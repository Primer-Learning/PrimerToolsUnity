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
    }
}
