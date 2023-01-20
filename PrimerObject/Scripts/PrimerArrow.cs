using System.Collections;
using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

public class PrimerArrow : PrimerObject
{
    [Header ("Controls")]
    [SerializeField] Vector3 headPosition;
    [SerializeField] float headBuffer;
    [SerializeField] Vector3 tailPosition = new Vector3(1, 1, 0);
    [SerializeField] float tailBuffer;

    [Header ("Object and transform references")]
    [SerializeField] PrimerObject shaftRoot = null;
    [SerializeField] Transform shaft = null;
    [SerializeField] Transform head = null;
    float currentLength;
    public Color Color {
        get {
            return head.GetComponentsInChildren<MeshRenderer>()[0].materials[0].color;
            // Assume head and shaft are the same color
        }
        set {
            head.GetComponentsInChildren<MeshRenderer>()[0].materials[0].color = value;
            shaft.GetComponentsInChildren<MeshRenderer>()[0].materials[0].color = value;
        }
    }


    void OnValidate() {
        if ( (tailPosition - headPosition).magnitude > 0 ) {
            SetFromTo(tailPosition, headPosition, endBuffer: headBuffer, startBuffer: tailBuffer);
        }
    }

    public void SetWidth(float width)
    {
        transform.localScale = Vector3.one * width;
    }

    public void SetLength(float length)
    {
        shaftRoot.transform.localPosition = new Vector3(
            length / transform.localScale.x,
            shaftRoot.transform.localPosition.y,
            shaftRoot.transform.localPosition.z
        );
        shaft.localScale = new Vector3(
            length / transform.localScale.x - 0.1f, //Leave a little room for the head to come to a point
            shaftRoot.transform.localScale.y,
            shaftRoot.transform.localScale.z
        );
        head.localPosition = new Vector3(
            -length / transform.localScale.x,
            shaftRoot.transform.localPosition.y,
            shaftRoot.transform.localPosition.z
        );
        currentLength = length;
    }

    public void SetFromTo(Vector3 from, Vector3 to, float endBuffer = 0f, float startBuffer = 0f)
    {
        //HandleBuffer
        float length = (from - to).magnitude;
        float startBufferFac = startBuffer / length;
        float endBufferFac = endBuffer / length;
        Vector3 bFrom = Vector3.Lerp(from, to, startBufferFac);
        Vector3 bTo = Vector3.Lerp(to, from, endBufferFac);
        length -= endBuffer + startBuffer;

        //Move object
        transform.localPosition = bTo;
        SetLength(length);
        //Assume we're looking at the local xy plane. This thing is flat.
        float rads = Mathf.Atan2((bFrom - bTo).y, (bFrom - bTo).x);
        transform.localRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);
    }

    public void AnimateFromTo(Vector3 from, Vector3 to, float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float endBuffer = 0f, float startBuffer = 0f)
    {
        StartCoroutine(animateFromTo(from, to, duration, ease, endBuffer, startBuffer));
    }
    private IEnumerator animateFromTo(Vector3 from, Vector3 to, float duration, EaseMode ease, float endBuffer = 0f, float startBuffer = 0f)
    {
        Vector3 oldPosition = transform.localPosition;
        Quaternion oldRotation = transform.localRotation;
        float oldLength = currentLength;

        //HandleBuffer
        float newLength = (from - to).magnitude;
        float startBufferFac = startBuffer / newLength;
        float endBufferFac = endBuffer / newLength;
        Vector3 bFrom = Vector3.Lerp(from, to, startBufferFac);
        Vector3 bTo = Vector3.Lerp(to, from, endBufferFac);
        newLength -= startBuffer + endBuffer;

        //get new rotation
        float rads = Mathf.Atan2((bFrom - bTo).y, (bFrom - bTo).x);
        Quaternion newRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);

        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = ease.Apply((Time.time - startTime) / duration);
            transform.localPosition = Vector3.Lerp(oldPosition, bTo, t);
            transform.localRotation = Quaternion.Slerp(oldRotation, newRotation, t);
            SetLength(Mathf.Lerp(oldLength, newLength, t));
            yield return null;
        }
        SetFromTo(bFrom, bTo); //Don't include buffer, since 'to' has already been altered
    }

    public override void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float delay = 0)
    {
        //Appear from the shaft, even though the point is our center here
        shaftRoot.ScaleUpFromZero(duration: duration, ease: ease, delay: delay);
    }

    public override void SetIntrinsicScale(Vector3 scale)
    {
        Debug.LogWarning("Arrows are animated by scaling a child object, setting current scale instead of intrinsic scale.");
        transform.localScale = scale;
    }
    public override void SetIntrinsicScale(float scale)
    {
        SetIntrinsicScale(Vector3.one * scale);
    }

    public override void SetColor(Color c)
    {
        Debug.LogWarning("Arrow color has been moved to a property. Just set PrimerArrow.Color normally.");
        Color = c;
    }
}
