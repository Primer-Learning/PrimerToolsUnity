using JetBrains.Annotations;
using Primer.Math;
using UnityEngine;

namespace Primer.Table
{
    [UsedImplicitly]
    public class PascalTrianglePlacer : ParametricEquation
    {
        public override Vector3 Evaluate(float t, float u)
        {
            var result = new Vector3(t - (u / 2), u, 0);
            Debug.Log($"{t} - {u}\n{result}");
            return result;
        }
    }
}
