using System;
using System.Collections.Generic;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/* TODO
 * (Core)
 * Add precision options for tic labels
 *
 * (When needed)
 * Make padding amount uniform instead of using fraction of each axis length (maybe)
 * Destroy unused tics, animating them to zero scale
 */
[ExecuteInEditMode]
public class Graph2 : PrimerBehaviour
{
    [FormerlySerializedAs("ticLabelDistanceVertical")]
    public float ticLabelDistance = 0.25f;
    // public float ticLabelDistanceHorizontal = 0.65f;
    [Range(0, 0.5f)]
    public float paddingFraction = 0.05f;
    public bool isRightHanded = true;
    public bool enableZAxis = true;

    [Header("Axes")]
    public Axis2 X;
    public Axis2 Y;
    public Axis2 Z;

    [Header("Prefabs")]
    public PrimerBehaviour arrowPrefab;
    public PrimerText2 primerTextPrefab;
    public Tic2 ticPrefab;

    void OnEnable() {
        OnValidate();
    }

    void OnValidate() {
        EnsureRightHanded();
        Z.hidden = !enableZAxis;
    }

    public void Regenerate() {
        if (X is not null) X.UpdateChildren();
        if (Y is not null) Y.UpdateChildren();
        if (Z is not null) Z.UpdateChildren();
    }

    bool lastRightHanded = true;
    void EnsureRightHanded() {
        if (isRightHanded == lastRightHanded) return;
        lastRightHanded = isRightHanded;

        Z.transform.rotation = isRightHanded
            ? Quaternion.Euler(0, 90, 0)
            : Quaternion.Euler(0, -90, 0);
    }
}
