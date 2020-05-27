using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimerBracket : PrimerObject
{
    [SerializeField] Transform lBar = null;
    [SerializeField] Transform rBar = null;
    [SerializeField] Transform lTip = null;
    [SerializeField] Transform rTip = null;
    [SerializeField] Transform lStem = null;
    [SerializeField] Transform rStem = null;

    Vector3 anchor;
    Vector3 lPoint;
    Vector3 rPoint;
    float lBarLength;
    float notBarLength;

    protected override void Awake() {
        base.Awake();
        //Store here from prefab in case of multiple adjustments
        notBarLength = 1 - rBar.localScale.x;
    }

    internal void SetPoints(Vector3 anchor, Vector3 lPoint, Vector3 rPoint) {
        this.anchor = anchor;
        this.lPoint = lPoint;
        this.rPoint = rPoint;

        Refresh();
    }

    internal void Refresh() {
        transform.localPosition = anchor;
        Vector3 toLPoint = lPoint - anchor;
        Vector3 toRPoint = rPoint - anchor;
        Vector3 lToR = rPoint - lPoint;
        Vector3 projected = Vector3.Project(-toLPoint, lToR);
        Vector3 anchorToBaseLine = toLPoint + projected;

        transform.localRotation = Quaternion.LookRotation(-anchorToBaseLine, Vector3.Cross(toRPoint, toLPoint));

        float stemToLine = anchorToBaseLine.magnitude;
        transform.localScale = new Vector3(stemToLine, stemToLine, stemToLine);

        float rLength = (toRPoint - anchorToBaseLine).magnitude / stemToLine;
        float lLength = (toLPoint - anchorToBaseLine).magnitude / stemToLine;


        lTip.localPosition = new Vector3(-lLength, 0, -1);
        rTip.localPosition = new Vector3(rLength, 0, -1);

        float lBarLength = lLength - notBarLength; 
        float rBarLength = rLength - notBarLength; 

        lBar.localScale = new Vector3(-lBarLength, lBar.localScale.y, lBar.localScale.z);
        rBar.localScale = new Vector3(rBarLength, rBar.localScale.y, rBar.localScale.z);
    }

    internal override void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        SetIntrinsicScale();
        base.ScaleUpFromZero(duration: duration, ease: ease);
    }

    public void SetColor(Color c) {
        lBar.GetComponent<MeshRenderer>().material.color = c;
        rBar.GetComponent<MeshRenderer>().material.color = c;
        lTip.GetComponent<MeshRenderer>().material.color = c;
        rTip.GetComponent<MeshRenderer>().material.color = c;
        lStem.GetComponent<MeshRenderer>().material.color = c;
        rStem.GetComponent<MeshRenderer>().material.color = c;
    }
}