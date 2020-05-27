using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class defining animations or other attributes that may be common to any object in a primer video
//Mostly easing
public class PrimerObject : MonoBehaviour
{
    protected Vector3 intrinsicScale = new Vector3 (1, 1, 1);
    private float rotateTowardsMaxAngle = 1;
    protected virtual void Awake() {}
    private Dictionary<Material, Color> originalColors = new Dictionary<Material, Color>();

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

    internal virtual void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        transform.localScale = Vector3.zero;
        StartCoroutine(scaleTo(intrinsicScale, duration, ease));
    }

    internal virtual void ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(scaleTo(Vector3.zero, duration, ease));
    }

    public void ScaleTo(Vector3 newScale, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(scaleTo(newScale, duration, ease));
    }
    public void ScaleTo(float newScale, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(scaleTo(new Vector3(newScale, newScale, newScale), duration, ease));
    }
    protected virtual IEnumerator scaleTo(Vector3 newScale, float duration, EaseMode ease) {
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

    public void MoveTo(Vector3 newPos, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(moveTo(newPos, duration, ease));
    }
    private IEnumerator moveTo(Vector3 newPos, float duration, EaseMode ease)
    {
        Vector3 initialPos = transform.localPosition;
        float startTime = Time.time;
        //Debug.Log(Time.frameCount);
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            transform.localPosition = Vector3.Lerp(initialPos, newPos, t);
            yield return null;
        }
        //Debug.Log(Time.frameCount);
        transform.localPosition = newPos; //Ensure we actually get exactly to newPos
    }

    public void RotateTo(Vector3 newEulerAngles, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        Quaternion newQuaternion = Quaternion.Euler(newEulerAngles);
        StartCoroutine(rotateTo(newQuaternion, duration, ease));
    }
    private IEnumerator rotateTo(Quaternion newQuaternion, float duration, EaseMode ease)
    {
        Quaternion initialRot = transform.localRotation;
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            transform.localRotation = Quaternion.Slerp(initialRot, newQuaternion, t);
            yield return null;
        }
        transform.localRotation = newQuaternion; //Ensure we actually get exactly to newQuaternion
    }

    public void LookToward(Vector3 toLookAt, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        Quaternion orientation = Quaternion.LookRotation(toLookAt - transform.position);
        StartCoroutine(rotateTo(orientation, duration, ease));
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
    public void WalkTo(Vector3 newPos, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, bool faceOriginal = false) {
        StartCoroutine(walkTo(newPos, duration, ease, faceOriginal));
    }
    private IEnumerator walkTo(Vector3 newPos, float duration, EaseMode ease, bool faceOriginal) {
        Quaternion originalAngle = transform.localRotation;
        //Turning should in general happen faster than displacing
        //One unit in the blob's scale, assuming it's uniform.
        float distance = (newPos - transform.position).magnitude;
        float turnDuration = duration * Mathf.Min(1 , transform.localScale.x / distance);

        MoveTo(newPos, duration, ease);
        LookToward(newPos, turnDuration, ease);
        if (faceOriginal) {
            yield return new WaitForSeconds(duration);
            RotateTo(originalAngle.eulerAngles);
        }
        yield return null;
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

    public virtual void ChangeColor(Color newColor, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                StartCoroutine(changeColor(mat, newColor, duration, ease));
            }
        }
    }

    public void FadeOut(float duration = 0.5f, EaseMode ease = EaseMode.None) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                StandardShaderUtils.ChangeRenderMode(mat, StandardShaderUtils.BlendMode.Transparent);
            }
        }
        ChangeAlpha(0, duration: duration, ease);
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
    }

    public void ChangeAlpha(float newAlpha, float duration = 0.5f, EaseMode ease = EaseMode.None) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
                Color newColor =  mat.color;
                newColor.a = newAlpha;
                StartCoroutine(changeColor(mat, newColor, duration, ease));
            }
        }
    }
    public void RevertColor(float duration = 0.5f, EaseMode ease = EaseMode.None) {
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs) {
            foreach (Material mat in mr.materials) {
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

}
