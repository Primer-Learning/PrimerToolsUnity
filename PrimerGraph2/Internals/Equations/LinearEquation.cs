using System;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class LinearEquation : ParametricEquation
    {
        public Vector3 start = Vector3.zero;
        public Vector3 end = new(10, 10, 0);

        public override Vector3 Evaluate(float t, float u) => new(
            (end.x - start.x) * u + start.x,
            (end.y - start.y) * (t * u) + start.y,
            (end.z - start.z) * t + start.z
        );
    }
}
