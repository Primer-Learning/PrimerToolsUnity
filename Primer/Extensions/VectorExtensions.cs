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

        public static void SetScale(this Transform transform, float value) => transform.SetScale(Vector3.one * value);

        public static void SetScale(this Transform transform, Vector3 scale, bool isGlobal = false)
        {
            if (isGlobal)
                transform.SetGlobalScale(scale);
            else
                transform.localScale = scale;
        }

        public static void SetGlobalScale(this Transform transform, Vector3 scale)
        {
            var parentScale = transform.parent is null
                ? Vector3.zero
                : transform.parent.lossyScale;

            transform.localScale = parentScale == Vector3.zero
                ? scale
                : scale.ScaleBy(parentScale.InvertScale());
        }

        public static Vector3 InvertScale(this Vector3 v, float @base = 1)
        {
            return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
        }

        public static Vector3 ScaleBy(this Vector3 v, Vector3 scale)
        {
            return new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
        }

        public static string ToCode(this Vector3 vector)
        {
            return $"new Vector3({vector.x}f, {vector.y}f, {vector.z}f)";
        }
    }
}
