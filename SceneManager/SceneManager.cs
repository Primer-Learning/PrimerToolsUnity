using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Scene;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;
using UnityEngine.Serialization;

public class SceneManager : PrimerObject 
{
    public static SceneManager instance;
    
    [Header("Camera default overrides")]
    public Camera cam;
    [FormerlySerializedAs("camRig")]
    public RenderToPng cameraRenderer;
    public bool useEditorVals = false;

    // RNG init
    public static System.Random sceneRandom;
    public static System.Random sceneRandom2; //Sometimes it's useful to have separate rng for visual purposes

    // Prefab vars, probably not necessary to do this here
    public PrimerArrow primerArrowPrefab;
    public PrimerBracket primerBracketPrefab;
    public PrimerText primerTextPrefab;
    public PrimerText primerCheckPrefab;
    public PrimerText primerXPrefab;
    public PrimerObject sunLight;

    protected override void Awake() {
        base.Awake();
        if (this.enabled) {
            if (instance != null) {
                Debug.LogError("More than one SceneManager instance enabled");
            }
            instance = this;
            
            //Initialize rng and print seed for tracking
            int seed = System.Environment.TickCount;
            // This is the main one
            sceneRandom = new System.Random(seed);
            // This is for purely visual stuff so messing with that doesn't affect rng state
            // Doesn't matter unless you set it in the Simulator class, but it's created here.
            sceneRandom2 = new System.Random(seed);
            Debug.Log("Seed: " + seed);

            // Load/assign common resources
            primerArrowPrefab = Resources.Load("arrowPrefab", typeof(PrimerArrow)) as PrimerArrow;
            primerBracketPrefab = Resources.Load("bracket", typeof(PrimerBracket)) as PrimerBracket;
            primerTextPrefab = Resources.Load("text", typeof(PrimerText)) as PrimerText;
            primerCheckPrefab = Resources.Load<PrimerText>("checkmark");
            primerXPrefab = Resources.Load<PrimerText>("x");
            if (sunLight == null) {
                sunLight = GameObject.Find("Directional Light").AddComponent<PrimerObject>();
                if (sunLight == null) {
                    Debug.LogWarning("Cannot find light");
                }
            }
        
            //Make camera rig if it doesn't exist
            if (cam == null) {
                cam = Camera.main;
                // Debug.Log(cam);
            }
            if (cameraRenderer == null) {
                // if (camRig == null) {
                    cameraRenderer = cam.gameObject.AddComponent<RenderToPng>();
                // }
                // camRig = cam.GetComponent<CameraRig>();
            }
            if (!useEditorVals) {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = PrimerColor.Gray;
                cam.backgroundColor = new Color(cam.backgroundColor.r, cam.backgroundColor.g, cam.backgroundColor.b, 0);
                sunLight.GetComponent<Light>().color = Color.white;
            }
        }
    }
}
