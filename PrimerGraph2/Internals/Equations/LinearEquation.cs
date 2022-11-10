using System;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class LinearEquation : ParametricEquation
    {
        public Vector3 start = Vector3.zero;
        public Vector3 end = new(10, 10, 10);

        public override Vector3 Evaluate(float t, float u) =>
            start + (end.x - start.x) * new Vector3(t, (t + u) / 2, u);
    }
}
