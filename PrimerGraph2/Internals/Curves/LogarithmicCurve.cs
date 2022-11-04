using System;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class LogarithmicCurve : Curve
    {
        public float offset = 1;
        public float whateverThisIsCalled = 2;

        public override float Evaluate(float x, float z) => Mathf.Log(x + offset, whateverThisIsCalled);
    }
}
