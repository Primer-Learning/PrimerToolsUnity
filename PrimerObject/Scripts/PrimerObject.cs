using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Class defining animations or other attributes that may be common to any object in a primer video
//Mostly easing
public class PrimerObject : MonoBehaviour
{
    protected Vector3 intrinsicScale = new Vector3 (1, 1, 1);
    private float rotateTowardsMaxAngle = 1;
    protected virtual void Awake() {}
    private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();
    IEnumerator rotationCoroutine;
    public Vector2 AnchoredPosition { 
        get { return GetComponent<RectTransform>().anchoredPosition;} 
        set { GetComponent<RectTransform>().anchoredPosition = value;} 
    }
    public Color EmissionColor {
        get {
            return GetComponentsInChildren<Renderer>()[0].materials[0].GetColor("_EmissionColor");
        } 
        set {
            Renderer[] mrs = GetComponentsInChildren<Renderer>();
            foreach (Renderer mr in mrs) {
                foreach (Material mat in mr.materials) {
                    mat.SetColor("_EmissionColor", value);
                    mat.EnableKeyword("_EMISSION");
                    // mat.color = Color.black;
                    // mat.SetFloat("_Glossiness", 0);
                }
            }
        } 
    }

    // This is a new way of doing easing. not actually implemented anywhere yet
    // public delegate ref TProperty KeyFunc<TProperty, TSelf>(TSelf self) where TSelf: PrimerObject;
    public delegate void OnFrameType();
    static void DoNothing(){}
    IEnumerator animateValue<TProperty>(string propertyName, TProperty newVal, float duration, EaseMode ease, OnFrameType onFrame) {
        var prop = this.GetType().GetProperty(propertyName);
        // Debug.Log($"prop = {prop}");
        // Debug.Log($"this.GetType() = {this.GetType()}");
        TProperty oldVal = (TProperty) prop.GetValue(this);
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            prop.SetValue( this, (TProperty) PrimerLerp(oldVal, newVal, t, typeof(TProperty)) );
            onFrame();
            // Debug.Log(key((TSelf)this));
            yield return null;
        }
        prop.SetValue(this, newVal);
        onFrame();
        // Debug.Log(key((TSelf)this));
    }
    public void AnimateValue<TProperty>(string propertyName, TProperty newVal, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, OnFrameType onFrameFunc = null) {
        if (onFrameFunc == null) { onFrameFunc = DoNothing; }
        StartCoroutine(animateValue<TProperty>(propertyName, newVal, duration, ease, onFrameFunc));
    }
    // The type checking and casting happens every frame, so it could probably be sped up a bit.
    object PrimerLerp(object oldVal, object newVal, float t, System.Type type) {
        if (type == typeof(int)) {
            // Casting to int then float looks silly, but it won't let me go straight to float
            return (int)Mathf.Lerp((float)(int)oldVal, (float)(int)newVal, t);
        }
        else if (type == typeof(float)) {
            return Mathf.Lerp((float)oldVal, (float)newVal, t);
        }
        else if (type == typeof(double)) {
            return (double)oldVal + ((double)newVal - (double)oldVal) * (double)t;
        }
        else if (type == typeof(Vector3)) {
            return Vector3.Lerp((Vector3)oldVal, (Vector3)newVal, t);
        }
        else if (type == typeof(Vector2)) {
            return Vector2.Lerp((Vector2)oldVal, (Vector2)newVal, t);
        }
        else if (type == typeof(Quaternion)) {
            return Quaternion.Slerp((Quaternion)oldVal, (Quaternion)newVal, t);
        }
        else if (type == typeof(Color)) {
            return Color.Lerp((Color)oldVal, (Color)newVal, t);
        }
        else {
            Debug.LogError("Can't lerp that type unless you fix me.");
            return null;
        }
    }
    public void StartCoroutineAfterDelay(IEnumerator aCoroutine, float delay) {
        StartCoroutine(startCoroutineAfterDelay(aCoroutine, delay));
    }
    public IEnumerator startCoroutineAfterDelay(IEnumerator aCoroutine, float delay) {
        yield return new WaitForSeconds(delay);
        StartCoroutine(aCoroutine);
    }

    public virtual void SetIntrinsicScale(Vector3 scale) {
        intrinsicScale = scale;
    }
    public virtual void SetIntrinsicScale(float scale) {
        intrinsicScale = new Vector3(scale, scale, scale);
    }
    public virtual void SetIntrinsicScale() {
        intrinsicScale = transform.localScale;
    }

    public Vector3 GetIntrinsicScale() {
        return intrinsicScale;
    }

    public virtual void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float delay = 0) {
        transform.localScale = Vector3.zero;
        StartCoroutine(scaleTo(intrinsicScale, duration, ease, delay: delay));
    }
    public void ScaleUpFromZeroStaggered(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float durationPerPiece = 0.1f) {
        StartCoroutine(scaleUpFromZeroStaggered(duration, ease, durationPerPiece));
    }
    IEnumerator scaleUpFromZeroStaggered (float duration, EaseMode ease, float durationPerPiece) {
        List<Transform> ts = GetComponentsInChildren<Transform>().ToList();
        List<Vector3> finalScales = new List<Vector3>();
        ts.Remove(this.transform);
        foreach (Transform t in ts) {
            finalScales.Add(t.localScale);
            t.localScale = Vector3.zero;
        }
        transform.localScale = intrinsicScale;

        float staggerAmount = duration * (1 - durationPerPiece) / ts.Count;
        float startTime = Time.time;
        int i = 0;
        while (Time.time < startTime + duration * (1 - durationPerPiece)) {
            while (i * staggerAmount + startTime < Time.time) {
                Transform t = ts[i];
                StartCoroutine(scaleTo(t, finalScales[i], durationPerPiece, ease));
                i++;
            }
            yield return null;
        }
        while (i < ts.Count - 1) {
            Transform t = ts[i];
            StartCoroutine(scaleTo(t, finalScales[i], durationPerPiece, ease));
            i++;
        }
    }

    public virtual void ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(scaleTo(Vector3.zero, duration, ease));
    }

    public virtual void Disappear(float duration = 0.5f, bool toPool = false, EaseMode ease = EaseMode.Cubic, float delay = 0) {
        if ((SceneManager.instance is Director && !((Director)SceneManager.instance).animating)) {
            duration = 0;
        }
        StartCoroutine(disappear(duration, toPool, ease, delay));
    }

    IEnumerator disappear(float duration, bool toPool, EaseMode ease, float delay) {
        if (delay > 0) { yield return new WaitForSeconds(delay); }
        StartCoroutine(scaleTo(Vector3.zero, duration, ease));
        yield return new WaitForSeconds(duration);
        if (toPool) {
            transform.parent = null;
            gameObject.SetActive(false);
        }
        else if (this.gameObject != null) { Destroy(this.gameObject); }
    }

    public void ScaleTo(Vector3 newScale, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, bool intrinsicY = false) {
        if (intrinsicY) {
            newScale[1] = intrinsicScale.y;
        }
        StartCoroutine(scaleTo(newScale, duration, ease));
    }
    public void ScaleTo(float newScale, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, bool intrinsicY = false) {
        ScaleTo(new Vector3(newScale, newScale, newScale), duration, ease, intrinsicY);
    }
    protected virtual IEnumerator scaleTo(Vector3 newScale, float duration, EaseMode ease, float delay = 0) {
        if (delay > 0) {
            yield return new WaitForSeconds(delay);
        }
        Vector3 initialScale = transform.localScale;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            transform.localScale = Vector3.Lerp(initialScale, newScale, t);
            yield return null;
        }

        transform.localScale = newScale; //Ensure we actually get exactly to newScale 
    }
    protected virtual IEnumerator scaleTo(Transform trans, Vector3 newScale, float duration, EaseMode ease) {
        Vector3 initialScale = trans.localScale;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            trans.localScale = Vector3.Lerp(initialScale, newScale, t);
            yield return null;
        }
        trans.localScale = newScale; //Ensure we actually get exactly to newScale 
    }
    protected virtual IEnumerator scaleTo(Vector3 newScale, float duration, EaseMode ease) {
        StartCoroutine(scaleTo(newScale, duration, ease, delay: 0));
        yield return null;
    }

    public void MoveBy(Vector3 disp, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, CoordinateFrame frame = CoordinateFrame.Local) {
        if (frame == CoordinateFrame.Local) {
            this.MoveTo(transform.localPosition + disp, duration, ease, frame);
        }
        else {
            this.MoveTo(transform.position + disp, duration, ease, frame);
        }
    }
    public void MoveTo(Vector3 newPos, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, CoordinateFrame frame = CoordinateFrame.Local) {
        StartCoroutine(moveTo(newPos, duration, ease, frame));
    }
    private IEnumerator moveTo(Vector3 newPos, float duration, EaseMode ease, CoordinateFrame frame)
    {
        Vector3 initialPos = Vector3.zero;
        if (frame == CoordinateFrame.Local) {
            initialPos = transform.localPosition;

            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                t = Helpers.ApplyNormalizedEasing(t, ease);
                transform.localPosition = Vector3.Lerp(initialPos, newPos, t);
                yield return null;
            }
            transform.localPosition = newPos; //Ensure we actually get exactly to newPos
        }
        else {
            initialPos = transform.position;

            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                t = Helpers.ApplyNormalizedEasing(t, ease);
                transform.position = Vector3.Lerp(initialPos, newPos, t);
                yield return null;
            }
            transform.position = newPos; //Ensure we actually get exactly to newPos
        }
    }
    public void MoveRectTo(Vector2 newPos, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        AnimateValue<Vector2>("AnchoredPosition", newPos, duration: duration, ease: ease);
    }

    public void MultiMove(List<Vector3> positions, float duration = 1, CoordinateFrame frame = CoordinateFrame.Local) {
        List<float> distances = new List<float>();
        Vector3 currentPosition = transform.localPosition;
        if (frame == CoordinateFrame.Global) {
            currentPosition = transform.position;
        }
        for (int i = 0; i < positions.Count; i++) {
            distances.Add( (positions[i] - currentPosition).magnitude );
            currentPosition = positions[i];
        }

        //Proportional distances
        float totalDistance = distances.Sum();
        List<float> distanceFractions = new List<float>();
        foreach (float d in distances) {
            distanceFractions.Add(d / totalDistance);
        }

        StartCoroutine(multiMove(positions, distanceFractions, duration, frame));
    }
    IEnumerator multiMove(List<Vector3> positions, List<float> timeFractions, float duration, CoordinateFrame frame) {
        // Not a completely smooth function, but should appear pretty smooth if the path isn't super zig-zaggy

        // First move eases into the motion
        MoveTo(positions[0], duration: duration * timeFractions[0], ease: EaseMode.SmoothIn, frame: frame);
        yield return new WaitForSeconds(duration * timeFractions[0]);

        // Middle moves are linear
        for (int i = 1; i < positions.Count - 1; i++) {
            MoveTo(positions[i], duration: duration * timeFractions[i], ease: EaseMode.None, frame: frame);
            yield return new WaitForSeconds(duration * timeFractions[i]);
        }

        // Final move eases out of the motion
        MoveTo(
            positions[positions.Count - 1], 
            duration: duration * timeFractions[timeFractions.Count - 1], 
            ease: EaseMode.SmoothOut, 
            frame: frame
        );
    }

    public void RotateTo(Vector3 newEulerAngles, float duration = 0.5f, CoordinateFrame frame = CoordinateFrame.Local, EaseMode ease = EaseMode.Cubic) {
        Quaternion newQuaternion = Quaternion.Euler(newEulerAngles);
        RotateTo(newQuaternion, duration: duration, frame: frame, ease: ease);
    }
    public void RotateTo(Quaternion newQuaternion, float duration = 0.5f, CoordinateFrame frame = CoordinateFrame.Local, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(rotateTo(newQuaternion, duration, frame, ease));
    }
    private IEnumerator rotateTo(Quaternion newQuaternion, float duration, CoordinateFrame frame, EaseMode ease)
    {   
        Quaternion initialRot = transform.localRotation;
        if (frame == CoordinateFrame.Global) {
            initialRot = transform.rotation;
        }
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            if (frame == CoordinateFrame.Local) {
                transform.localRotation = Quaternion.Slerp(initialRot, newQuaternion, t);
            }
            else {
                transform.rotation = Quaternion.Slerp(initialRot, newQuaternion, t);
            }
            yield return null;
        }
        
        if (frame == CoordinateFrame.Local) {
            transform.localRotation = newQuaternion; //Ensure we actually get exactly to newQuaternion
        }
        else {
            transform.rotation = newQuaternion; //Ensure we actually get exactly to newQuaternion

        }
    }

    public void LookToward(Vector3 toLookAt, float duration = 0.5f, bool xzPlane = false, EaseMode ease = EaseMode.Cubic, CoordinateFrame frame = CoordinateFrame.Local) {
        Quaternion orientation = Quaternion.identity;

        Vector3 lookVec = toLookAt - transform.localPosition;
        if (frame == CoordinateFrame.Global) {
            lookVec = toLookAt - transform.position;
        }
        if (xzPlane) {
            lookVec = new Vector3(lookVec.x, 0, lookVec.z);
        }
        if (lookVec != Vector3.zero) {
            if (frame == CoordinateFrame.Local) {
                orientation = Quaternion.LookRotation(lookVec);
            }
            else {
                orientation = Quaternion.LookRotation(lookVec);
            }
        }
        StartCoroutine(rotateTo(orientation, duration, frame, ease));
    }

    public void RotateByEuler(Vector3 eulerRotation, float duration = 0.5f, EaseMode ease = EaseMode.SmoothStep) {
        StartCoroutine(rotateByEuler(eulerRotation, duration, ease));
    }
    private IEnumerator rotateByEuler(Vector3 eulerRotation, float duration, EaseMode ease)
    {
        //Final rot for setting at the end. Applied rotation goes first so it appears in 
        //parent/world space
        Quaternion finalRot = Quaternion.Euler(eulerRotation) * transform.localRotation;
        // Plan is to interpolate euler angle into pieces to get small rotations,
        // then apply those rotations as quaternions one at a time

        Vector3 rotSoFar = Vector3.zero;

        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            Vector3 nextRotSoFar = Vector3.Lerp(Vector3.zero, eulerRotation, t);
            transform.Rotate(nextRotSoFar - rotSoFar, Space.World);
            rotSoFar = nextRotSoFar;
            yield return null;
        }
        transform.localRotation = finalRot;
    }

    public void RotateTowardsWithInertia(Quaternion target, bool global = false) {
        //Assumes it's being called constantly, otherwise inertia doesn't really make sense
        //The point here is to be able to "turn on" facing without big orientation discontinuities
        //or a fixed rotation cap. Specifically, letting graph labels follow a quickly rotating camera.
        rotateTowardsMaxAngle++;
        if (global) {
            transform.rotation = Quaternion.RotateTowards(transform.localRotation, target, rotateTowardsMaxAngle);
        }
        else {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, target, rotateTowardsMaxAngle);
        }
        
        //If we're not there yet, go faster. If we are there, decay.
        //There might be a more robust way to do this, but it seems to work for my current purpose.
        if (transform.localRotation == target) {
            rotateTowardsMaxAngle--;
        }
        else {
            rotateTowardsMaxAngle++;
        }
    }
    public void RotateContinuously(float revsPerSec, Vector3 axis) {
        rotationCoroutine = rotateContinuously(revsPerSec, axis);
        StartCoroutine(rotationCoroutine);
    }
    public void StopContinuousRotation() {
        StopCoroutine(rotationCoroutine);
    }
    IEnumerator rotateContinuously(float revsPerSec, Vector3 axis) {
        // Intended to run for the life of the scene
        while (true) {
            float rotDegs = Time.deltaTime * revsPerSec * 360;
            transform.rotation *= Quaternion.AngleAxis(rotDegs, axis);
            yield return null;
        }
    }

    // A more dizzy-looking rotation
    public void RotateContinuouslyEuler(Vector3 eulerAngleIncrement) {
        StartCoroutine(rotateContinuouslyEuler(eulerAngleIncrement));
    }
    IEnumerator rotateContinuouslyEuler(Vector3 eulerAngleIncrement) {
        // Intended to run for the life of the scene
        Vector3 euler = transform.rotation.eulerAngles;
        while (true) {
            euler += eulerAngleIncrement;
            transform.rotation = Quaternion.Euler(euler);
            yield return null;
        }
    }

    public void WalkTo(Vector3 newPos, float duration = 0.5f, bool xzPlane = false, EaseMode ease = EaseMode.Cubic, bool faceOriginal = false, CoordinateFrame frame = CoordinateFrame.Local) {
        StartCoroutine(walkTo(newPos, duration, xzPlane, ease, faceOriginal, frame));
    }
    private IEnumerator walkTo(Vector3 newPos, float duration, bool xzPlane, EaseMode ease, bool faceOriginal, CoordinateFrame frame) {
        Quaternion originalAngle = transform.localRotation;
        float distance = 0;
        float turnDuration = 0;
        if (frame == CoordinateFrame.Local) {
            //Turning should in general happen faster than displacing
            //One unit in the blob's scale, assuming it's uniform.
            distance = (newPos - transform.localPosition).magnitude;
            turnDuration = duration * Mathf.Min(1 , transform.localScale.x / distance);
        }
        else {
            //Turning should in general happen faster than displacing
            //One unit in the blob's scale, assuming it's uniform.
            distance = (newPos - transform.position).magnitude;
            turnDuration = duration * Mathf.Min(1 , transform.localScale.x / distance);
        }

        MoveTo(newPos, duration, ease, frame);
        LookToward(newPos, turnDuration, xzPlane, ease, frame);
        if (faceOriginal) {
            yield return new WaitForSeconds(duration);
            RotateTo(originalAngle.eulerAngles);
        }
        yield return null;
    }
    public void MultiWalk(List<Vector3> positions, float duration = 1, bool xzPlane = false, bool faceOriginal = false, CoordinateFrame frame = CoordinateFrame.Local) {
        // Get original facing direction in case faceOriginals
        Quaternion rotation = Quaternion.identity;
        if (faceOriginal) { rotation = transform.rotation; }

        List<float> distances = new List<float>();
        Vector3 currentPosition = transform.localPosition;
        if (frame == CoordinateFrame.Global) {
            currentPosition = transform.position;
        }
        for (int i = 0; i < positions.Count; i++) {
            distances.Add( (positions[i] - currentPosition).magnitude );
            currentPosition = positions[i];
        }

        //Proportional distances
        float totalDistance = distances.Sum();
        List<float> distanceFractions = new List<float>();
        foreach (float d in distances) {
            distanceFractions.Add(d / totalDistance);
        }


        StartCoroutine(multiWalk(positions, distanceFractions, duration, xzPlane, faceOriginal, rotation, frame));
    }
    IEnumerator multiWalk(List<Vector3> positions, List<float> timeFractions, float duration, bool xzPlane, bool faceOriginal, Quaternion prevRotation, CoordinateFrame frame) {
        // Not a completely smooth function, but should appear pretty smooth if the path isn't super zig-zaggy

        // First move eases into the motion
        WalkTo(positions[0], duration: duration * timeFractions[0], ease: EaseMode.SmoothIn, xzPlane: xzPlane, frame: frame);
        yield return new WaitForSeconds(duration * timeFractions[0]);

        // Middle moves are linear
        for (int i = 1; i < positions.Count - 1; i++) {
            WalkTo(positions[i], duration: duration * timeFractions[i], ease: EaseMode.None, xzPlane: xzPlane, frame: frame);
            yield return new WaitForSeconds(duration * timeFractions[i]);
        }

        // Final move eases out of the motion
        WalkTo(
            positions[positions.Count - 1], 
            duration: duration * timeFractions[timeFractions.Count - 1], 
            xzPlane: xzPlane,
            ease: EaseMode.SmoothOut, 
            frame: frame
        );
        yield return new WaitForSeconds(duration * timeFractions[timeFractions.Count - 1]);
        if (faceOriginal) {
            RotateTo(prevRotation, frame: CoordinateFrame.Global);
        }
    }

    public void AnimateLightIntensityTo(float intensity, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(animateLightIntensityTo(intensity, duration, ease));
    }

    private IEnumerator animateLightIntensityTo(float intensity, float duration, EaseMode ease) {
        Light l = GetComponent<Light>();
        float initialIntensity = l.intensity;
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            l.intensity = Mathf.Lerp(initialIntensity, intensity, t);
            yield return null;
        }
        l.intensity = intensity;
    }

    public void Pulse(float factor = 1.2f, float duration = 1, float attack = 0.5f, float decay = 0.5f) {
        StartCoroutine(pulse(factor, duration, attack, decay));
    }

    private IEnumerator pulse(float factor, float duration, float attack, float decay) {
        Vector3 initialScale = transform.localScale;
        this.ScaleTo(initialScale * factor, duration: attack);
        yield return new WaitForSeconds(duration - decay);
        this.ScaleTo(initialScale, duration: decay);
    }
    // Todo: Make this "AnimateColor" instead of "ChangeColor" for more consistent and clear naming
    public virtual void ChangeColor(Color newColor, Renderer r, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        StartCoroutine(changeColor(r.material, newColor, duration, ease));
    }
    public virtual void ChangeColor(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                StartCoroutine(changeColor(mat, newColor, duration, ease));
            }
        }
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in smrs) {
            foreach (Material mat in smr.materials) {
                StartCoroutine(changeColor(mat, newColor, duration, ease));
            }
        }
    }
    public virtual void ChangeColorStaggered(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        StartCoroutine(changeColorStaggered(newColor, duration, ease));
    }
    IEnumerator changeColorStaggered(Color newColor, float duration, EaseMode ease) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        float durationPerPiece = 1f/10; 
        float staggerAmount = duration * (1 - durationPerPiece) / mrs.Length;

        float startTime = Time.time;
        int i = 0;
        while (Time.time < startTime + duration * (1 - durationPerPiece)) {
            while (i * staggerAmount + startTime < Time.time) {
                MeshRenderer mr = mrs[i];
                foreach (Material mat in mr.materials) {
                    StartCoroutine(changeColor(mat, newColor, durationPerPiece, ease));
                }
                i++;
            }
            yield return null;
            // foreach (MeshRenderer mr in mrs) {
            //     // Could definitely add control here
            //     // Objects with many childre (e.g., DNA) will likely have staggerAmount round up to one frame
            //     foreach (Material mat in mr.materials) {
            //         yield return new WaitForSeconds(staggerAmount);
            //         StartCoroutine(changeColor(mat, newColor, duration / 2, ease));
            //     }
            // }
        }
        while (i < mrs.Length - 1) {
                MeshRenderer mr = mrs[i];
                foreach (Material mat in mr.materials) {
                    StartCoroutine(changeColor(mat, newColor, durationPerPiece, ease));
                }
                i++;
        }
    }
    public virtual void TempColorChange(Color newColor, float duration = 1, float attack = 0.5f, float decay = 0.5f, EaseMode ease = EaseMode.None) {
        StartCoroutine(tempColorChange(newColor, duration, attack, decay, ease));
    }
    public virtual IEnumerator tempColorChange(Color newColor, float duration, float attack, float decay, EaseMode ease) {
        Color oldColor = GetComponentsInChildren<MeshRenderer>()[0].material.color;
        ChangeColor(newColor, duration: attack, ease: ease);
        yield return new WaitForSeconds(duration - attack - decay);
        ChangeColor(oldColor, duration: decay, ease: ease);
    }
    public virtual void SetColor(Color newColor, Renderer r) {
        r.material.color = newColor;
    }
    public virtual void SetColor(Color newColor, bool onlyFirstMaterial = false) {
        Renderer[] mrs = GetComponentsInChildren<Renderer>();
        if (onlyFirstMaterial) {
            mrs[0].material.color = newColor;
        }
        else {
            SetColor(newColor);
        }
    }
    public virtual void SetColor(Color newColor) {
        Renderer[] mrs = GetComponentsInChildren<Renderer>();
        foreach (Renderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                mat.color = newColor;
            }
        }
    }
    public virtual void SetEmissionColor(Color newColor, Renderer r) {
        r.material.SetColor("_EmissionColor", newColor);
        r.material.EnableKeyword("_EMISSION");
        // r.material.color = Color.black;
        // r.material.SetFloat("_Glossiness", 0);
    }
    public virtual void AnimateEmissionColor(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        AnimateValue<Color>("EmissionColor", newColor, duration: duration, ease: ease);
    }
    public virtual void FadeOut(float newAlpha = 0, float duration = 0.5f, float delay = 0, EaseMode ease = EaseMode.None, List<PrimerObject> exemptions = null) {
        if (exemptions == null) { exemptions = new List<PrimerObject>(); }
        StartCoroutine(fadeOut(newAlpha, duration, delay, ease, exemptions));
    }
    IEnumerator fadeOut(float newAlpha, float duration, float delay, EaseMode ease, List<PrimerObject> exemptions) {
        yield return new WaitForSeconds(delay);

        List<Transform> trueExemptions = new List<Transform>();
        foreach (PrimerObject p in exemptions) {
            trueExemptions.AddRange(p.GetComponentsInChildren<Transform>());
        }
        // List<Transform> exemptionParents = new List<Transform>();
        // foreach (PrimerObject e in exemptions) {
        //     exemptionParents.Add(e.transform.parent);
        //     e.transform.parent = null;
        // }
        // List<Transform> children = GetComponentsInChildren<Transform>().ToList();
        // foreach (Transform
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        // foreach (Transform t in children) {
        foreach (MeshRenderer mr in mrs) {
            if (trueExemptions.Contains(mr.gameObject.GetComponent<Transform>())) { continue; }
            // MeshRenderer mr = t.gameObject.GetComponent<MeshRenderer>();
            foreach (Material mat in mr.materials) {
                StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Transparent);
            }
        }
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in smrs) {
            if (trueExemptions.Contains(smr.gameObject.GetComponent<Transform>())) { continue; }
            foreach (Material mat in smr.materials) {
                StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Transparent);
            }
        }
        // for (int i = 0; i < exemptions.Count; i++) {
        //     exemptions[i].transform.parent = exemptionParents[i];
        // }
        ChangeAlpha(newAlpha, duration: duration, ease, trueExemptions);
    }
    public void FadeIn(float duration = 0.5f, EaseMode ease = EaseMode.None) {
        StartCoroutine(fadeIn(duration, ease));
    }
    private IEnumerator fadeIn(float duration = 0.5f, EaseMode ease = EaseMode.None) {
        ChangeAlpha(1, duration: duration, ease);
        yield return new WaitForSeconds(duration);
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Opaque);
            }
        }
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in smrs) {
            foreach (Material mat in smr.materials) {
                StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Opaque);
            }
        }
    }

    public void ChangeAlpha(float newAlpha, float duration = 0.5f, EaseMode ease = EaseMode.None, List<Transform> exemptions = null) {
        if (exemptions == null) { exemptions = new List<Transform>(); }
        // foreach (PrimerObject e in exemptions) {
        //     e.transform.parent = null;
        // }
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            if (exemptions.Contains(mr.gameObject.GetComponent<Transform>())) { continue; }
            foreach (Material mat in mr.materials) {
                Color newColor =  mat.color;
                newColor.a = newAlpha;
                StartCoroutine(changeColor(mat, newColor, duration, ease));
            }
        }
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in smrs) {
            if (exemptions.Contains(smr.gameObject.GetComponent<Transform>())) { continue; }
            foreach (Material mat in smr.materials) {
                Color newColor =  mat.color;
                newColor.a = newAlpha;
                StartCoroutine(changeColor(mat, newColor, duration, ease));
            }
        }
        // foreach (PrimerObject e in exemptions) {
        //     e.transform.parent = transform;
        // }
    }
    public void RevertColor(float duration = 0.5f, EaseMode ease = EaseMode.None) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                StartCoroutine(changeColor(mat, originalColors[mat], duration, ease));
            }
        }
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in smrs) {
            foreach (Material mat in smr.materials) {
                StartCoroutine(changeColor(mat, originalColors[mat], duration, ease));
            }
        }
    }
    public IEnumerator changeColor(Material mat, Color newColor, float duration, EaseMode ease) {
        //Assumes simple structure where the first meshrenderer in the hierarchy is what you want.
        Color initialColor = mat.color;
        if (!originalColors.ContainsKey(mat)) {
            originalColors.Add(mat, initialColor);
        }
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            mat.color = Color.Lerp(initialColor, newColor, t);
            yield return null;
        }
        mat.color = newColor;
    }

    public void ChangeMatTiling(Vector2 newTiling, float duration, EaseMode ease = EaseMode.Cubic) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                StartCoroutine(changeMatTiling(mat, newTiling, duration, ease));
            }
        }
        SkinnedMeshRenderer[] smrs = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in smrs) {
            foreach (Material mat in smr.materials) {
                StartCoroutine(changeMatTiling(mat, newTiling, duration, ease));
            }
        }
    }
    public IEnumerator changeMatTiling(Material mat, Vector2 newTiling, float duration, EaseMode ease) {
        Vector2 initialTiling = mat.mainTextureScale;
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            mat.mainTextureScale = Vector2.Lerp(initialTiling, newTiling, t);
            yield return null;
        }
        mat.mainTextureScale = newTiling;
    }
}
