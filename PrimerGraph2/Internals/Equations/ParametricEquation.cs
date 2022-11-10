using UnityEngine;

namespace Primer.Graph
{
    public abstract class ParametricEquation
    {
        public virtual Vector3 Evaluate(float t) => Evaluate(t, 0);
        public abstract Vector3 Evaluate(float t, float u);
    }
}
