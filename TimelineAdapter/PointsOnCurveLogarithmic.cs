using JetBrains.Annotations;
using UnityEngine;

[UsedImplicitly]
public class PointsOnCurveLogarithmic : PointsOnCurve
{
    public override float Curve(float x) => Mathf.Log(x + 1, 2);
}
