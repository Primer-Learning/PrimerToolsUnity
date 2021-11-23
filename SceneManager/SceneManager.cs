using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using UnityEditor;
using TMPro;

public class SceneManager : PrimerObject 
{
    public static SceneManager instance;
    protected CameraRig camRig;
    protected GameObject camSwivel;
    internal static System.Random sceneRandom;
    internal static System.Random sceneRandom2; //Sometimes it's useful to have separate rng for visual purposes

    internal PrimerArrow primerArrowPrefab;
    internal PrimerBracket primerBracketPrefab;
    internal PrimerText textPrefab;
    internal Graph graphPrefab;
    internal PrimerObject sunLight;
    public Color backgroundColor;

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
            primerArrowPrefab = Resources.Load("arrow", typeof(PrimerArrow)) as PrimerArrow;
            primerBracketPrefab = Resources.Load("bracket", typeof(PrimerBracket)) as PrimerBracket;
            textPrefab = Resources.Load("text", typeof(PrimerText)) as PrimerText;
            graphPrefab = Resources.Load("Graph", typeof(Graph)) as Graph;
            if (sunLight == null) {
                sunLight = GameObject.Find("Directional Light").AddComponent<PrimerObject>();
                if (sunLight == null) {
                    Debug.LogWarning("Cannot find light");
                }
                sunLight.GetComponent<Light>().color = Color.white;
            }
        
            //Make Camera
            camSwivel = GameObject.Find("Cam Swivel");
            if (camSwivel == null) {
                camSwivel = new GameObject("Cam Swivel");
            }
            camRig = camSwivel.AddComponent<CameraRig>();
        }
    }
}