using System;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class LogarithmicYEquation : ParametricEquation
    {
        public float offset = 1;

        [Range(2, 10)]
        public float _base = 2;

        public Vector3 start = Vector3.zero;
        public Vector3 end = new(10, 10, 0);

        public override Vector3 Evaluate(float t, float u) =>
            start + (end.x - start.x) * new Vector3(t, Mathf.Log((t + u) / 2 + offset, _base), u);
    }
}
