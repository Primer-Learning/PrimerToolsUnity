using UnityEngine;
using System.IO;



public class CameraRig : PrimerObject
{
    // Class that handles camera movement and recording
    // Makes it easy to rotate the camera around a center

    internal Camera cam;
    RenderTexture rt = null;
    Texture2D image = null;

    Vector3 swivelOrigin = Vector3.zero;
    Vector3 swivelAxis = Vector3.up;
    private float swivelAngle = 0;
    public float SwivelAngle {
        get { return swivelAngle; }
        set {
            float angleDiff = value - swivelAngle;
            transform.RotateAround(swivelOrigin, swivelAxis, angleDiff);
            // transform.Rotate(swivelAxis, angleDiff);
            swivelAngle = value;
        }
    }

    void Awake() {
        cam = SceneManager.instance.cam; 
    }
    public void GrabLight() {
        GameObject light = GameObject.Find("Directional Light");
        light.transform.parent = transform;
    }
    public void ZoomTo(float distance, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        MoveTo(-transform.forward * distance + swivelOrigin, duration, ease);
    }

    public void SwivelTo(float angle, Vector3? point = null, Vector3? axis = null, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        if (point != null) { swivelOrigin = point.Value; }
        if (axis != null) { swivelAxis = axis.Value; }
        AnimateValue<float>("SwivelAngle", angle, duration: duration, ease: ease);
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
