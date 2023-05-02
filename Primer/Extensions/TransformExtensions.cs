using UnityEngine;

namespace Primer
{
    public static class TransformExtensions
    {
        public static void RemoveAllChildren(this Transform transform)
        {
            var children = GetChildren(transform);

            foreach (var child in children)
                child.Dispose();
        }

        public static Transform[] GetChildren(this Transform transform)
        {
            var children = new Transform[transform.childCount];

            for (var i = 0; i < transform.childCount; i++)
                children[i] = transform.GetChild(i);

            return children;
        }

        public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null, bool global = false)
        {
            var pos = global ? transform.position : transform.localPosition;
            var newPos = new Vector3(x ?? pos.x, y ?? pos.y, z ?? pos.z);
            transform.SetPosition(newPos, global);
        }

        public static void SetPosition(this Transform transform, Vector3 newPosition, bool global = false)
        {
            if (global) {
                transform.position = newPosition;
            }
            else {
                transform.localPosition = newPosition;
            }
        }

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

        public static void SetDefaultsNoScale(this Transform transform, bool saveIntrinsicScale = false)
        {
            SetDefaults(transform, Vector3.one, saveIntrinsicScale);
        }

        public static void SetDefaults(this Transform transform, Vector3? scale = null, bool saveIntrinsicScale = false)
        {
            if (saveIntrinsicScale)
                transform.GetPrimer().FindIntrinsicScale();

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = scale ?? Vector3.one;
        }
    }
}
