using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

/// <summary>
/// Defines and manages a camera rig centered on an object. Usually will be an empty at the origin.
/// </summary>
public class CameraRig : PrimerObject
{
    public Camera cam;
    public PrimerObject camObject; //Public because sometimes I'll just want to manipulate the camera itself without intermediary methods

    bool recording = false;
    internal string frameOutDir = null;
    RenderTexture rt = null;
    Texture2D image = null;
    // TODO: Probably make thes accessible from the editor
    int resWidth = 2560;
    int resHeight = 1440;

    internal void SetUp(bool solidColor = true) {
        if (cam == null) { 
            cam = Camera.main; 
        }
        cam.transform.parent = transform;
        if (solidColor) {
            cam.clearFlags = CameraClearFlags.SolidColor;
            Color bCol = SceneManager.instance.backgroundColor;
            bCol.a = 0; //Ensure this for the recorder output
            cam.backgroundColor = bCol;
        }
        camObject = cam.gameObject.AddComponent<PrimerObject>();
    }
    public void GoToStandardPositions() {
        cam.transform.localPosition = new Vector3 (0, 0, -10);
        cam.transform.localRotation = Quaternion.identity;

        transform.localPosition = new Vector3(0, 1, 0);
        transform.localRotation = Quaternion.Euler(16, 0, 0);
    }
    public void GrabLight() {
        GameObject light = GameObject.Find("Directional Light");
        light.transform.parent = cam.transform;
    }
    internal void MoveRigCenter(Vector3 c) {
        // Move rig after storing global position
        Vector3 prevGlobal = transform.position;
        transform.localPosition = c;

        // Now move the child camera to put it back where it was
        cam.transform.position -= transform.position - prevGlobal;
    }

    public void ZoomTo(float distance, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        camObject.MoveTo(new Vector3 (camObject.transform.localPosition.x, camObject.transform.localPosition.y, -distance), duration, ease);
    }

    internal void RenderToPNG(string path) {
        // I will admit that I'm not sure why I need to do the setup and cleanup
        // each frame, but if I don't, I just get black frames.

        // Set up camera
        rt = new RenderTexture(resWidth, resHeight, 24);
        cam.targetTexture = rt;
        RenderTexture.active = rt;
        image = new Texture2D(resWidth, resHeight, TextureFormat.RGBA32, false);
        
        // Render to image texture
        cam.Render();
        image.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);

        // Save png
        byte[] bytes = image.EncodeToPNG();
        File.WriteAllBytes(path, bytes);

        // Clean up
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(image);
    }
    internal void StartRecording(int everyNFrames = 1) {
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
        StartCoroutine(startRecording(path, everyNFrames));
    }
    IEnumerator startRecording(string path, int everyNFrames) {
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
                RenderToPNG(fileName);
                framesSaved++;
            }
            framesSeen++;
        }
    }
    internal void StopRecording(float waitTime = 0) {
        StartCoroutine(stopRecording(waitTime));
    }
    IEnumerator stopRecording(float waitTime) {
        if (waitTime > 0) {
            yield return new WaitForSeconds(waitTime);
        }
        recording = false;

        yield return null;
    }
}
