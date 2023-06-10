using Cysharp.Threading.Tasks;
using Primer;
using Primer.Scene;
using Primer.Timeline;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class AutoPlayScenes : EditorWindow
{
    private static string[] scenes =
    {
        "Test scene",
        "Scene0/Scene0",
        "Scene1/Scene1",
        "Scene4/Scene4",
        "Scene5/Scene5",
        "Scene6/Scene6 - Testing and bar graph"
    };
    private static int sceneIndex = 0;
    private static QualityPreset currentQuality;

    // Preset options
    [MenuItem("Custom/AutoPlayScenes/Minimal")]
    static void InitMinimal()
    {
        currentQuality = QualityPreset.Minimal;
        Init();
    }

    [MenuItem("Custom/AutoPlayScenes/Low")]
    static void InitLow()
    {
        currentQuality = QualityPreset.Low;
        Init();
    }

    [MenuItem("Custom/AutoPlayScenes/High")]
    static void InitHigh()
    {
        currentQuality = QualityPreset.High;
        Init();
    }

    [MenuItem("Custom/AutoPlayScenes/Publish")]
    static void InitPublish()
    {
        currentQuality = QualityPreset.Publish;
        Init();
    }
    static void Init()
    {
        Debug.Log("Init");
        
        // Load the first scene
        EditorSceneManager.OpenScene("Assets/Scenes/" + scenes[sceneIndex] + ".unity");
        
        SetUpRenderer();
        
        // Enter play mode
        EditorApplication.EnterPlaymode();
        EditorApplication.playModeStateChanged += LogPlayModeState;
    }
    
    private static void LogPlayModeState(PlayModeStateChange state)
    {
        // When exiting play mode
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // Increment the scene index
            sceneIndex++;
            if (sceneIndex >= scenes.Length)
            {
                // If we've gone through all the scenes, remove this callback
                EditorApplication.playModeStateChanged -= LogPlayModeState;
                return;
            }
            // Load the next scene
            EditorSceneManager.OpenScene("Assets/Scenes/" + scenes[sceneIndex] + ".unity");
            
            SetUpRenderer();
            
            // Enter play mode
            EditorApplication.EnterPlaymode();
        }
    }
    
    private static void SetUpRenderer()
    {
        // ChatGPT wants to set all the cameras, and why not?
        foreach (var camera in Camera.allCameras)
        {
            var renderToPng = camera.GetComponent<RenderToPng>();
            if (renderToPng != null)
            {
                renderToPng.enabled = true;
                renderToPng.qualityPreset = currentQuality;
                
                // These are all very likely for non-test renders
                renderToPng.recordingStartTimeInSeconds = 0;
                renderToPng.endTimeInSeconds = 0;
                renderToPng.transparentBackground = true;
                renderToPng.omitRepeatingFrames = false;
                renderToPng.fillGapsInPreviousTake = false;

                if (sceneIndex != scenes.Length - 1)
                {
                    renderToPng.endSound = null;
                }
            }
        }
    }
}