using UnityEngine;

namespace Primer
{
    public static class VectorExtensions
    {
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

        public static Vector3 InvertScale(this Vector3 v)
        {
            return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
        }

        public static Vector3 ScaleBy(this Vector3 v, Vector3 scale)
        {
            return new Vector3(v.x * scale.x, v.y * scale.y, v.z * scale.z);
        }
    }
}
