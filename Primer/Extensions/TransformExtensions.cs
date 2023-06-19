using UnityEngine;

namespace Primer
{
    public static class TransformExtensions
    {
        public static void CopyTo(this Transform source, Transform target, bool copyParent = true, Vector3? offsetPosition = null)
        {
            if (copyParent)
                target.parent = source.parent;

            target.localPosition = offsetPosition is null
                ? source.localPosition
                : source.localPosition + source.localRotation * Vector3.Scale(offsetPosition.Value, source.localScale);

            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }

        public static Transform SetDefaults(this Transform transform, Vector3? scale = null, bool saveIntrinsicScale = false)
        {
            if (saveIntrinsicScale)
                transform.GetPrimer().FindIntrinsicScale();

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = scale ?? Vector3.one;

            return transform;
        }
    }
}
