using UnityEngine;
using System.IO;



public class CameraRig : PrimerObject
{
    // Class that handles camera movement and recording
    // Makes it easy to rotate the camera around a center

    internal Camera cam;
    RenderTexture rt = null;
    Texture2D image = null;

    // This lets us control the camera in the inspector as if it had a parent transform.
    // Currently, these override changes made directly to the transform.
    // Ideally, changing either would update the other.
    void OnValidate() {
        SwivelOrigin = swivelOrigin;
        Swivel = Quaternion.Euler(swivelEuler);
        Distance = distance;
    }
    void OnDrawGizmos() {
        Gizmos.DrawSphere(swivelOrigin, 0.1f);
    }

    [SerializeField] Vector3 swivelOrigin;
    private Vector3 oldSwivelOrigin; // This allows the SwivelOrigin setter to notice the change when called from OnValidate
    public Vector3 SwivelOrigin {
        get { return swivelOrigin; }
        set {
            Vector3 posDiff = value - oldSwivelOrigin;
            transform.position += posDiff;
            swivelOrigin = value;
            oldSwivelOrigin = swivelOrigin;
        }
    }
    public Vector3 swivelEuler; // For editing from the inspector through OnValidate
    private Quaternion swivel;
    public Quaternion Swivel {
        get { return swivel; }
        set {
            Quaternion diffRotation = Quaternion.Inverse(swivel) * value;
            float angle;
            Vector3 axis;
            diffRotation.ToAngleAxis(out angle, out axis);
            transform.RotateAround(swivelOrigin, axis, angle);
            swivel = transform.localRotation;
            swivelEuler = swivel.eulerAngles;
        }
    }
    internal bool faceSwivel = true;
    [SerializeField] float distance = 10;
    public float Distance {
        get { 
            return (transform.position - swivelOrigin).magnitude;
        }
        set {
            if (faceSwivel) {
                transform.position = swivelOrigin + swivel * Vector3.back * value;
            }
            else {
                Vector3 directionVector = (transform.position - swivelOrigin).normalized;
                transform.position = directionVector * value + swivelOrigin;
            }
            distance = value;
        }
    }
    
    void Start() {
        cam = SceneManager.instance.cam;
    }
    public void GrabLight() {
        GameObject light = GameObject.Find("Directional Light");
        light.transform.parent = transform;
    }
    public void MoveCenterTo(Vector3 newCenter, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        AnimateValue<Vector3>("SwivelOrigin", newCenter, duration: duration, ease: ease);
    }
    public void SwivelTo(Quaternion rotation, Vector3? point = null, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        if (point != null) { swivelOrigin = point.Value; }
        AnimateValue<Quaternion>("Swivel", rotation, duration: duration, ease: ease);
    }
    public void ZoomTo(float distance, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        AnimateValue<float>("Distance", distance, duration: duration, ease: ease);
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
