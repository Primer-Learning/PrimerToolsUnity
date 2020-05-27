using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Director : MonoBehaviour
{
    public static Director instance;
    private bool paused;
    protected CameraRig camRig;
    protected GameObject camSwivel;
    internal float timeBehind; //For adjusting effective timing after events with flexible timing
    protected bool waiting;
    private SceneBlock currentBlock;

    [SerializeField] TextMeshProUGUI timeDisplay = null;
    [SerializeField] TextMeshProUGUI frameDisplay = null;
    [SerializeField] TextMeshProUGUI timeScaleDisplay = null;
    public TextMeshProUGUI customDisplay1 = null;
    public TextMeshProUGUI customDisplay2 = null;
    public static System.Random sceneRandom;
    public static List<SceneBlock> schedule = new List<SceneBlock>();
    protected virtual void DefineSchedule() {}

    [SerializeField] protected PrimerArrow primerArrowPrefab = null;
    [SerializeField] protected PrimerBracket primerBracketPrefab = null;
    [SerializeField] protected PrimerText textPrefab = null;
    [SerializeField] protected Graph graphPrefab = null;

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
    }

    protected virtual void Awake()
    {   
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this.gameObject);
        }

        //Set capture rate, since we're making frames here  w
        Time.captureFramerate = 60;

        //Initialize rng
        int seed = System.Environment.TickCount;
        sceneRandom = new System.Random(seed);
        Debug.Log("Seed: " + seed);
      
        camSwivel = GameObject.Find("Cam Swivel");
        //Make Camera
        if (camSwivel == null) {
            camSwivel = new GameObject("Cam Swivel");
        }
        camRig = camSwivel.AddComponent<CameraRig>();

        DefineSchedule();
        schedule.Sort((x,y) => x.scheduledStartTime.CompareTo(y.scheduledStartTime));

        //Make scenes start right away without having to manually subtract the timing for each
        float firstStart = schedule[0].scheduledStartTime;
        timeBehind = -firstStart;
    }

    internal void StopWaiting() {
        //TODO: figure out why scheduled blocks are sometimes off by a frame. Doesn't super matter, though.
        if (waiting) {
            float extraTimeTaken = Time.time - currentBlock.actualStartTime - currentBlock.expectedDuration;
            //Update how far behind we're running (can be negative)
            timeBehind += extraTimeTaken;
            waiting = false;
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
            }
            else {
                Time.timeScale = 0;
                paused = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            Time.timeScale /= 2;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            Time.timeScale *= 2;
        }
    }

    //Start scenes when it's time.
    //Doing this in LateUpdate stops some off-by-one-frame errors from building up, but I don't doubt
    //there's a cleaner way to make sure coroutines finish on the right frame.
    void LateUpdate()
    {
        if (!waiting && schedule.Count > 0) {
            float sceneTime = Time.time - timeBehind; //If things take extra time, we run behind
            this.currentBlock = schedule[0];
            if (currentBlock.scheduledStartTime <= sceneTime) {
                //If it's time, pluck it from the list and start it
                schedule.Remove(currentBlock); 
                StartCoroutine(currentBlock.delegateIEnumerator());
                if (currentBlock.flexible && schedule.Count > 0) {
                    waiting = true;
                    currentBlock.actualStartTime = Time.time;
                    currentBlock.expectedDuration = schedule[0].scheduledStartTime - currentBlock.scheduledStartTime;
                }
            }
        }
    }
}

public class WaitUntilSceneTime : CustomYieldInstruction
{
    float timeToWaitUntil;
    public override bool keepWaiting
    {
        get
        {
            return Time.time - Director.instance.timeBehind < timeToWaitUntil;
        }
    }

    public WaitUntilSceneTime(float t)
    {
        timeToWaitUntil = t;
    }
}