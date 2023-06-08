using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class VectorExtensions
    {
        public static Vector3 Average(this IEnumerable<Vector3> vectors)
        {
            return Average(vectors.ToArray());
        }

        public static Vector3 Average(params Vector3[] vectors)
        {
            var total = vectors.Aggregate((current, vec) => current + vec);
            return total / vectors.Length;
        }

        public static string ToCode(this Vector3 vector)
        {
            return $"new Vector3({vector.x}f, {vector.y}f, {vector.z}f)";
        }

        public static Vector3 ElementWiseMultiply(this Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
        }
    }
}
