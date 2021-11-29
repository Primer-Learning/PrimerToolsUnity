using UnityEngine;
using System.IO;



public class CameraRig : PrimerObject
{
    // Class that handles camera movement and recording
    // Makes it easy to rotate the camera around a center

    internal Camera cam;
    internal PrimerObject camObject;
    bool recording = false;
    RenderTexture rt = null;
    Texture2D image = null;

    internal void SetUp(bool solidColor = true) {
        cam = SceneManager.instance.cam; 
        cam.transform.parent = transform;
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

    internal void RenderToPNG(string path, int resWidth, int resHeight) {
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
}
