using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CameraRig : PrimerObject
{
    // Class that handles camera movement and recording
    // Makes it easy to rotate the camera around a center

    [SerializeField] bool gridOverlay;
    [SerializeField] int xDivisions = 3;
    [SerializeField] int yDivisions = 3;
    [SerializeField] int lineWidth = 5;
    [SerializeField] Slider sliderPrefab = null;
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
        UpdateGridOverlay();
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
    void UpdateGridOverlay() {
        if (gridOverlay) {
            Canvas canvas = null;
            if (GameObject.Find("Canvas") == null) {
                GameObject canvasGO = new GameObject();
                canvasGO.name = "Canvas";
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            else { canvas = GameObject.Find("Canvas").GetComponent<Canvas>(); }

            GridLayoutGroup vGroup = null;
            if (GameObject.Find("vGridGroup") == null) {
                GameObject vGroupGO = new GameObject();
                vGroupGO.transform.parent = canvas.transform;
                vGroupGO.name = "vGridGroup";
                vGroup = vGroupGO.AddComponent<GridLayoutGroup>();

                RectTransform vgrt = vGroupGO.GetComponent<RectTransform>();
                vgrt.anchorMin = Vector2.zero;
                vgrt.anchorMax = Vector2.one;
                vgrt.offsetMin = Vector2.zero;
                vgrt.offsetMax = Vector2.zero;

                vGroup.childAlignment = TextAnchor.MiddleCenter;
                vGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                vGroup.constraintCount = 1;
            }
            else { vGroup = GameObject.Find("vGridGroup").GetComponent<GridLayoutGroup>(); }
            vGroup.cellSize = new Vector2(lineWidth, canvas.GetComponent<RectTransform>().sizeDelta.y);
            vGroup.spacing = new Vector3( (float) (canvas.GetComponent<RectTransform>().sizeDelta.x - lineWidth) / xDivisions - lineWidth, 0);

            List<Image> vDividers = vGroup.GetComponentsInChildren<Image>().ToList();
            while (vDividers.Count > xDivisions + 1) {
                DestroyImmediate(vDividers[0].gameObject);
                vDividers.RemoveAt(0);
            }
            for (int i = vDividers.Count; i < xDivisions + 1; i++) {
                Image image = new GameObject().AddComponent<Image>();
                image.gameObject.AddComponent<CanvasRenderer>();
                image.transform.parent = vGroup.transform;
            }

            GridLayoutGroup hGroup = null;
            if (GameObject.Find("hGridGroup") == null) {
                GameObject hGroupGO = new GameObject();
                hGroupGO.transform.parent = canvas.transform;
                hGroupGO.name = "hGridGroup";
                hGroup = hGroupGO.AddComponent<GridLayoutGroup>();

                RectTransform hgrt = hGroupGO.GetComponent<RectTransform>();
                hgrt.anchorMin = Vector2.zero;
                hgrt.anchorMax = Vector2.one;
                hgrt.offsetMin = Vector2.zero;
                hgrt.offsetMax = Vector2.zero;

                hGroup.childAlignment = TextAnchor.MiddleCenter;
                hGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                hGroup.constraintCount = 1;
            }
            else { hGroup = GameObject.Find("hGridGroup").GetComponent<GridLayoutGroup>(); }
            hGroup.cellSize = new Vector2(canvas.GetComponent<RectTransform>().sizeDelta.x, lineWidth);
            hGroup.spacing = new Vector3( 0, (float) (canvas.GetComponent<RectTransform>().sizeDelta.y - lineWidth) / yDivisions - lineWidth);

            List<Image> hDividers = hGroup.GetComponentsInChildren<Image>().ToList();
            while (hDividers.Count > yDivisions + 1) {
                DestroyImmediate(hDividers[0].gameObject);
                hDividers.RemoveAt(0);
            }
            for (int i = hDividers.Count; i < yDivisions + 1; i++) {
                Image image = new GameObject().AddComponent<Image>();
                image.gameObject.AddComponent<CanvasRenderer>();
                image.transform.parent = hGroup.transform;
            }
        }
        if (!gridOverlay) {
            GameObject vGroup = GameObject.Find("vGridGroup");
            if (vGroup != null) { DestroyImmediate(vGroup); }
            GameObject hGroup = GameObject.Find("hGridGroup");
            if (hGroup != null) { DestroyImmediate(hGroup); }
        }
    }
}
