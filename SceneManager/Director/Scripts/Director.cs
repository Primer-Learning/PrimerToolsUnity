using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

public enum DirectorMode {
    Normal, //Normal mode, keeps game time and real time close
    SkipSimAnimations, //Skips some yield statements so we don't wait for animations
    ConstantFrameRate //Sets captureFramerate ensure constant framerate output, real and game time diverge
}
public class Director : SceneManager
{
    private bool paused;
    internal bool animating = true;
    internal float timeBehind; //For adjusting effective timing after events with flexible timing

    [Header("Mode select")]
    [SerializeField] protected DirectorMode mode;

    [Header("Recording options")]
    [SerializeField] bool recordOnPlay;
    public int videoFrameRate = 60;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int everyNFrames = 1;
    string frameOutDir = null;
    bool recording = false;

    [Header("Testing options")]
    [SerializeField] bool lightBackground = false;

    [Header("Scene parameters")]
    // So subclasses will have this header automatically
    // But it doesn't work!

    // Some reference displays that might be useful, but I rarely use anymore
    TextMeshProUGUI timeDisplay = null;
    TextMeshProUGUI frameDisplay = null;
    TextMeshProUGUI timeScaleDisplay = null;    

    // Media references to test how animations sync with voice or other video
    ReferenceScreen referenceScreenPrefab = null;
    internal AudioSource voiceOverReference;
    internal static List<SceneBlock> schedule = new List<SceneBlock>();
    internal AudioSource doneAlarm;

    protected virtual void DefineSchedule() {}

    protected virtual void Start() {
        if (lightBackground) {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.gray;
        }
        if (recordOnPlay) {
            StartRecording();
        }

        // override in subclass
        StartCoroutine(playScene());
    }
    internal IEnumerator playScene() {
        for (int i = 0; i < schedule.Count; i++) {
            SceneBlock sb = schedule[i];

            yield return new WaitUntilSceneTime(sb.scheduledStartTime);

            sb.actualStartTime = Time.time;

            if (voiceOverReference != null && mode == DirectorMode.Normal) {
                voiceOverReference.time = sb.scheduledStartTime;
                voiceOverReference.Play();
            }
            if (schedule.Count > i + 1) {
                sb.expectedDuration = schedule[i + 1].scheduledStartTime - sb.scheduledStartTime;
            }
            yield return sb.delegateIEnumerator();
        }
        List<PrimerObject> allPOs = Object.FindObjectsOfType<PrimerObject>().ToList();
        foreach (PrimerObject po in allPOs) {
            po.Disappear();
        }
        if (mode == DirectorMode.ConstantFrameRate) {
            StopRecording(delay: 1); //Delay by one second to give a buffer
        }
        if (doneAlarm != null) {
            doneAlarm.loop = false;
            doneAlarm.Play();
            while (doneAlarm.isPlaying) {
                yield return null;
            }
        }
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }
    internal ReferenceScreen MakeReferenceScreen(VideoClip clip = null) {
        ReferenceScreen rs = Instantiate(referenceScreenPrefab);
        if (clip != null) {
            rs.screen.clip = clip;
            rs.screen.gameObject.GetComponent<MeshRenderer>().material.SetFloat("_Glossiness", 0);
        }
        return rs;
    }
    public class SceneBlock {
        //Define delegate type
        public delegate IEnumerator DelegateIEnumerator();
        //Declare instance of new delegate type, assigned in constructor
        public DelegateIEnumerator delegateIEnumerator;
        //Time management fields
        public float scheduledStartTime; //When it's supposed to start

        //Fields for managing the fact that some SceneBlocks will have flexible duration, mostly sims.
        //This is probably more complex than strictly necessary, but I want to be able to
        //mindlessly enter timing values based on a finished voiceover recording that already 
        //leaves time for sims and then, during editing, insert or remove silence based on the actual 
        //final duration of the sim.
        //Some refactoring could probably help, in any case.
        public bool flexible; //If true, Update() will wait for delegateIEnumerator to finish and then alter the sceneTime clock accordingly.
        public float actualStartTime; //Assigned when delegateIEnumator is started
        public float expectedDuration; //Assigned based on difference in scheduled times

        //Constructor
        public SceneBlock(float time, DelegateIEnumerator delegateIEnumerator, bool flexible = false) {
            this.scheduledStartTime = time;
            this.delegateIEnumerator = delegateIEnumerator;
            this.flexible = flexible;
            schedule.Add(this);
        }
        public SceneBlock(int m, float s, DelegateIEnumerator delegateIEnumerator, bool flexible = false) {
            float time = m * 60 + s;
            this.scheduledStartTime = time;
            this.delegateIEnumerator = delegateIEnumerator;
            this.flexible = flexible;
            schedule.Add(this);
        }
    }
    internal DirectorMode GetDirectorMode() {
        return mode;
    }
    internal void SetDirectorMode(DirectorMode newMode) {
        mode = newMode;
        switch (mode) 
        {
            case DirectorMode.SkipSimAnimations:
                Time.captureFramerate = 0;
                animating = false;
                Debug.LogWarning("Animations may be messed up. This is a trial run.");
                break;
            case DirectorMode.Normal:
                Time.captureFramerate = 0;
                animating = true;
                break;
            case DirectorMode.ConstantFrameRate:
                Time.captureFramerate = videoFrameRate;
                animating = true;
                break;
        }
    }
    protected override void Awake() {   
        base.Awake();
        if (this.enabled) {
            if (voiceOverReference == null) {
                voiceOverReference = this.gameObject.AddComponent<AudioSource>();
                voiceOverReference.clip = Resources.Load("voiceover", typeof(AudioClip)) as AudioClip;
                voiceOverReference.playOnAwake = false;
            }
            if (doneAlarm == null) {
                doneAlarm = this.gameObject.AddComponent<AudioSource>();
                doneAlarm.clip = Resources.Load("zelda_secret", typeof(AudioClip)) as AudioClip;
                doneAlarm.playOnAwake = false;
            }

            SetDirectorMode(mode);
            DefineSchedule();
            schedule.Sort((x,y) => x.scheduledStartTime.CompareTo(y.scheduledStartTime));

            //Make scenes start right away without having to manually subtract the timing for each
            if (schedule.Count > 0) {
                float firstStart = schedule[0].scheduledStartTime;
                timeBehind = -firstStart;
            }
        }
    }

    protected virtual void Update() {
        if (timeDisplay != null) {
            timeDisplay.text = (Time.time - timeBehind).ToString("0.0");
        }
        if (frameDisplay != null) {
            //Frame count if no acceleration, for previewing where framecounts will be in final recording
            frameDisplay.text = (Time.time * 60).ToString("0");
        }
        if (timeScaleDisplay != null) {
            timeScaleDisplay.text = (Time.timeScale).ToString("0") + "x";
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (paused) {
                Time.timeScale = 1;
                paused = false;
                if (((Director)SceneManager.instance).voiceOverReference != null) {
                    ((Director)SceneManager.instance).voiceOverReference.Play();
                }
            }
            else {
                Time.timeScale = 0;
                paused = true;
                if (((Director)SceneManager.instance).voiceOverReference != null) {
                    ((Director)SceneManager.instance).voiceOverReference.Pause();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            Time.timeScale /= 2;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Time.timeScale = Mathf.Min(Time.timeScale * 2, 100);
        }
    }

    void StartRecording(int everyNFrames = 1) {
        cam.enabled = false;

        // In frameOutDir, make a folder with the director's name, if it doesn't exist
        if (frameOutDir == null) {
            frameOutDir = Directory.GetCurrentDirectory();
            Debug.LogWarning($"Frame capture directory not set. Setting to {frameOutDir}.");
        }
        string path = Path.Combine(frameOutDir, "png", Director.instance.gameObject.name + "_recordings");
        Directory.CreateDirectory(path);

        // Make a new folder with a take number
        string takeDir = "";
        if (takeDir == "") {
            int index = 0;
            string basePath = path;
            while (Directory.Exists(path)){
                index++; // It starts with one...
                                        // One thing. I don't know why.
                                        // It doesn't even matter how hard you try.
                                        // Keep that in mind, I designed this rhyme
                                        // to explain in due time...
                         // All I know...
                path = Path.Combine(basePath, $"take {index}");
            }
        }
        Directory.CreateDirectory(path);
        
        // Pass this folder to the coroutine to let it save each frame.
        recording = true;
        StartCoroutine(startRecording(path));
    }
    IEnumerator startRecording(string path) {
        //Save frame with frame numbe
        int framesSeen = 0;
        int framesSaved = 0;
        while (recording) {
            yield return new WaitForEndOfFrame();
            if (Time.frameCount > 999999) { Debug.LogWarning("y tho"); }
            if (framesSeen % everyNFrames == 0) {
                string fileName = framesSaved.ToString("000000");
                fileName += ".png";
                fileName = Path.Combine(path, fileName);
                camRig.RenderToPNG(fileName, resolutionWidth, resolutionHeight);
                framesSaved++;
            }
            framesSeen++;
        }
    }
    internal void StopRecording(float delay = 0) {
        StartCoroutine(stopRecording(delay));
    }
    IEnumerator stopRecording(float delay) {
        if (delay > 0) {
            yield return new WaitForSeconds(delay);
        }
        recording = false;
        yield return null;
    }
}

public class WaitUntilSceneTime : CustomYieldInstruction {
    float timeToWaitUntil;
    public override bool keepWaiting
    {
        get
        {   
            bool toKeepWaiting = Time.time - ((Director)SceneManager.instance).timeBehind < timeToWaitUntil;
            if (toKeepWaiting == false && ((Director)SceneManager.instance).voiceOverReference != null) {
                ((Director)SceneManager.instance).voiceOverReference.time = timeToWaitUntil;
            }
            return toKeepWaiting;
        }
    }

    public WaitUntilSceneTime(float t) // Scene time in seconds
    {
        timeToWaitUntil = t;
        float lateAmount = Time.time - ((Director)SceneManager.instance).timeBehind - timeToWaitUntil;
        if (lateAmount > 0) {
            Debug.LogWarning($"Instruction to WaitUntilSceneTime({t}) comes too late by {lateAmount}s.");
        }
    }
    public WaitUntilSceneTime(int m, float s) // Minutes and seconds because I'm tired of doing this mentally
    {
        timeToWaitUntil = m * 60 + s;
        float lateAmount = Time.time - ((Director)SceneManager.instance).timeBehind - timeToWaitUntil;
        if (lateAmount > 0) {
            Debug.LogWarning($"Instruction to WaitUntilSceneTime({m}:{s}) comes too late by {lateAmount}s.");
        }
    }
}
