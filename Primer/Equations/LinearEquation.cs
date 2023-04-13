using System;
using UnityEngine;

namespace Primer
{
    [Serializable]
    public class LinearEquation : ParametricEquation
    {
        public Vector3 start = Vector3.zero;
        public Vector3 end = new(10, 10, 10);

        public override Vector3 Evaluate(float t, float u) =>
            start + Vector3.Scale(end - start, new Vector3(t, (t + u) / 2, u));
    }
}
