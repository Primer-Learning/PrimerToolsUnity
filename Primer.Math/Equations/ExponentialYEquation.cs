using System;
using UnityEngine;

namespace Primer.Math
{
    [Serializable]
    public class ExponentialYEquation : ParametricEquation
    {
        public Vector3 end = new(10, 10, 10);
        [Range(1, 100)]
        public float exponent = 2;

        public Vector3 start = Vector3.zero;

        public override Vector3 Evaluate(float t, float u) =>
            start + (end.x - start.x) * new Vector3(t, Mathf.Pow((t + u) / 2, exponent), u);
    }
}
