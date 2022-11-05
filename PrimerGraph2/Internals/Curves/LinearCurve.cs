using System;

namespace Primer.Graph
{
    [Serializable]
    public class LinearCurve : Curve
    {
        public float offset;
        public float multiplier = 1;

        public override float Evaluate(float x, float z) => x * multiplier + offset;
    }
}
