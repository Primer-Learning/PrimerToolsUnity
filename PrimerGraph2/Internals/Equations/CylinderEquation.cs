using JetBrains.Annotations;
using Primer.Graph;
using UnityEngine;

namespace DefaultNamespace
{
    [UsedImplicitly]
    public class CylinderEquation : ParametricEquation
    {
        public Vector3 offset = new(3, 0, 3);
        public float radius = 1;
        public float length = 10;

        public override Vector3 Evaluate(float t, float u) =>
            offset + new Vector3(
            Mathf.Cos(t * Mathf.PI * 2) * radius,
            u * length,
            Mathf.Sin(t * Mathf.PI * 2) * radius
        );
    }
}
