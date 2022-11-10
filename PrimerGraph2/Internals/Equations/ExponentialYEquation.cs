using System;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class ExponentialYEquation : ParametricEquation
    {
        public float exponent = 2;
        public Vector3 start = Vector3.zero;
        public Vector3 end = new(10, 10, 0);

        public override Vector3 Evaluate(float t, float u) {
            var point = Vector3.Lerp(start, end, t);
            point.y = Mathf.Lerp(start.y, end.y, Mathf.Pow(t + u, exponent));
            return point;
        }
    }
}
