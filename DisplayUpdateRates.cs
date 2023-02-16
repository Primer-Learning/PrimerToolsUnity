using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

// Adapted from Unity's FixedUpdate documentation
// Added game time per real time to test the effect of Time.captureFramerate

public class DisplayUpdateRates : MonoBehaviour
{
    public bool showInGameView = false;
    
    private float updateCount = 0;
    private float fixedUpdateCount = 0;
    [ShowInInspector, ReadOnly]
    private float updateUpdateCountPerSecond;
    [ShowInInspector, ReadOnly]
    private float updateFixedUpdateCountPerSecond;
    private float lastRealTime;
    [ShowInInspector, ReadOnly]
    private float deltaRealTime;

    void Awake()
    {
        // Uncommenting this will cause framerate to drop to 10 frames per second.
        // This will mean that FixedUpdate is called more often than Update.
        //Application.targetFrameRate = 10;
        StartCoroutine(Loop());
        lastRealTime = Time.realtimeSinceStartup;
    }

    // Increase the number of calls to Update.
    void Update()
    {
        updateCount += 1;
    }

    // Increase the number of calls to FixedUpdate.
    void FixedUpdate()
    {
        fixedUpdateCount += 1;
    }

    // Show the number of calls to both messages.
    void OnGUI()
    {
        if (showInGameView)
        {
            GUIStyle fontSize = new GUIStyle(GUI.skin.GetStyle("label"));
            fontSize.fontSize = 24;
            GUI.Label(new Rect(100, 50, 500, 100), "Updates per game second: " + updateUpdateCountPerSecond.ToString(),
                fontSize);
            GUI.Label(new Rect(100, 100, 500, 100),
                "FixedUpdates per game second: " + updateFixedUpdateCountPerSecond.ToString(), fontSize);
            GUI.Label(new Rect(100, 150, 500, 100), "Game seconds per real second: " + (1 / deltaRealTime).ToString(),
                fontSize);
        }
    }

    // Update both CountsPerSecond values every second.
    IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            updateUpdateCountPerSecond = updateCount;
            updateFixedUpdateCountPerSecond = fixedUpdateCount;

            updateCount = 0;
            fixedUpdateCount = 0;
            
            var newRealtime = Time.realtimeSinceStartup;
            deltaRealTime = newRealtime - lastRealTime;
            lastRealTime = newRealtime;
            // Debug.Log(deltaRealTime);
        }
    }
}
