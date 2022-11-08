using UnityEngine;

namespace Primer.Graph
{
    public abstract class ParametricEquation
    {
        public abstract Vector3 Evaluate(float t);
    }
}
