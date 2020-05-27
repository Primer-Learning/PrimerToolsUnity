using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimerArrow : PrimerObject
{
    [SerializeField] PrimerObject shaftRoot = null;
    [SerializeField] Transform shaft = null;
    [SerializeField] Transform head = null;

    float currentLength;

    public void SetLength(float length) {
        shaftRoot.transform.localPosition = new Vector3 (
            length / transform.localScale.x,
            shaftRoot.transform.localPosition.y,
            shaftRoot.transform.localPosition.z
        );
        shaft.localScale = new Vector3 (
            length / transform.localScale.x - 0.1f, //Leave a little room for the head to come to a point
            shaftRoot.transform.localScale.y,
            shaftRoot.transform.localScale.z
        );
        head.localPosition = new Vector3 (
            -length / transform.localScale.x,
            shaftRoot.transform.localPosition.y,
            shaftRoot.transform.localPosition.z
        );
        currentLength = length;
    }

    public void SetFromTo(Vector3 from, Vector3 to, float buffer = 0f) {
        //HandleBuffer
        float length = (from - to).magnitude;
        float bufferFac = buffer / length;
        to = Vector3.Lerp(to, from, bufferFac);
        length -= buffer;

        //Move object
        transform.localPosition = to;
        SetLength(length);
        //Assume we're looking at the local xy plane. This thing is flat.
        float rads = Mathf.Atan2((from - to).y, (from - to).x);
        transform.localRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);
    }

    public void AnimateFromTo(Vector3 from, Vector3 to, float buffer = 0, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(animateFromTo(from, to, buffer, duration, ease));
    }
    private IEnumerator animateFromTo(Vector3 from, Vector3 to, float buffer, float duration, EaseMode ease) {
        Vector3 oldPosition = transform.localPosition;
        Quaternion oldRotation = transform.localRotation;
        float oldLength = currentLength;

        //HandleBuffer
        float newLength = (from - to).magnitude;
        float bufferFac = buffer / newLength;
        to = Vector3.Lerp(to, from, bufferFac);
        newLength -= buffer;

        //get new rotation
        float rads = Mathf.Atan2((from - to).y, (from - to).x);
        Quaternion newRotation = Quaternion.Euler(0, 0, rads * Mathf.Rad2Deg);

        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            transform.localPosition = Vector3.Lerp(oldPosition, to, t);
            transform.localRotation = Quaternion.Slerp(oldRotation, newRotation, t);
            SetLength(Mathf.Lerp(oldLength, newLength, t));
            yield return null;
        }
        SetFromTo(from, to, buffer);
    }

    internal override void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic)
    {
        //Appear from the shaft, even though the point is our center here
        shaftRoot.ScaleUpFromZero(duration: duration, ease: ease);
    }
    
    public override void SetIntrinsicScale(Vector3 scale) {
        Debug.LogError("Don't set intrinsic scale for arrows. Arrows are animated by scaling a child object.");
    }

    public void SetColor(Color c) {
        head.GetComponent<MeshRenderer>().material.color = c;
        shaft.GetComponent<MeshRenderer>().material.color = c;
    }
}