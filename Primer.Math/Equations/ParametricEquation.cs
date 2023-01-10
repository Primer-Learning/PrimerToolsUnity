using UnityEngine;

namespace Primer.Math
{
    public abstract class ParametricEquation
    {
        public virtual Vector3 Evaluate(float t) => Evaluate(t, t);

        public abstract Vector3 Evaluate(float t, float u);
    }
}
