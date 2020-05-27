using UnityEngine;

/// <summary>
/// Defines and manages a camera rig centered on an object. Usually will be an empty at the origin.
/// </summary>
public class CameraRig : PrimerObject
{
    public Camera cam;
    public PrimerObject camObject; //Public because sometimes I'll just want to manipulate the camera itself without intermediary methods

    protected override void Awake() {
        if (cam == null) { 
            cam = Camera.main; 
        }
        cam.transform.parent = transform;
        cam.clearFlags = CameraClearFlags.SolidColor;
        Color bCol = new Color(0.2f, 0.2f, 0.2f, 1);
        bCol.a = 0; //Ensure this for the recorder output
        cam.backgroundColor = bCol;
        cam.usePhysicalProperties = true;
        //cam.focalLength = 50f;

        camObject = cam.gameObject.AddComponent<PrimerObject>();
    }
    public void GoToStandardPositions() {
        cam.transform.localPosition = new Vector3 (0, 0, -10);
        cam.transform.localRotation = Quaternion.identity;

        transform.localPosition = new Vector3(0, 1, 0);
        transform.localRotation = Quaternion.Euler(16, 0, 0);
    }
    public void GrabLight() {
        //Attach light to camera and align it to camera view. Mostly good for illuminating front-facing text, which is maybe not needed since Unity has a canvas. /shrug
        GameObject light = GameObject.Find("Directional Light");
        light.transform.parent = cam.transform;
        light.transform.localPosition = Vector3.zero;
        light.transform.localRotation = Quaternion.identity;
    }

    //TODO: Method for keeping rig in a constant direction when attached to a moving object.s

    public void ZoomTo(float distance, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        camObject.MoveTo(new Vector3 (0, 0, -distance), duration, ease);
    }
}