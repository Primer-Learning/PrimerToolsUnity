using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DNA : PrimerObject
{
    [SerializeField] GameObject strandOnePrefab = null;
    [SerializeField] GameObject strandTwoPrefab = null;
    internal PrimerObject strand1;
    internal PrimerObject strand2;
    internal Color? color = null;
    protected override void Awake() {
        base.Awake();
    }
    
    internal void GenerateStrands(float duration = 1) {
        if (strand1 == null) {
            strand1 = Instantiate(strandOnePrefab).MakePrimerObject();
            if (color != null) {
                strand1.SetColor((Color)color);
            }
            strand1.transform.parent = transform;
            strand1.transform.localPosition = Vector3.zero;
            strand1.transform.localRotation = Quaternion.identity;
            strand1.ScaleUpFromZeroStaggered(duration: duration);
        }
        if (strand2 == null) {
            strand2 = Instantiate(strandTwoPrefab).MakePrimerObject();
            if (color != null) {
                strand2.SetColor((Color)color);
            }
            strand2.transform.parent = transform;
            strand2.transform.localPosition = Vector3.zero;
            strand2.transform.localRotation = Quaternion.identity;
            strand2.ScaleUpFromZeroStaggered(duration: duration);
        }
    }

    internal PrimerObject ReleaseStrand() {
        PrimerObject strand = strand1;
        strand1 = null;
        return strand;
    }
    internal void AcceptStrand(PrimerObject strand, float duration = 1) {
        strand1 = strand;
        strand1.transform.parent = transform;
        strand1.MoveTo(Vector3.zero, duration: duration);
        strand1.RotateTo(Quaternion.identity, duration: duration);
        strand1.ScaleTo(Vector3.one, duration: duration);
    }
}
