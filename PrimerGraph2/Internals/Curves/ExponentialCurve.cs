using System;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class ExponentialCurve : Curve
    {
        public float offset;
        public float exponent = 2;

        public override float Evaluate(float x, float z) => Mathf.Pow(x, exponent) + offset;
    }
}
