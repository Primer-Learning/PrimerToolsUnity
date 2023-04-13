using UnityEngine;

namespace Primer.Animation
{
    public static class TransformExtensions
    {
        // Convenience overload method for scaling to multiples of Vector3.one
        public static Tween ScaleTo(this Transform transform, float newScale)
        {
            return transform.ScaleTo(newScale * Vector3.one);
        }

        public static Tween ScaleTo(this Transform transform, Vector3 newScale)
        {
            var initial = transform.localScale;

            return initial == newScale
                ? Tween.noop
                : new Tween(t => transform.localScale = Vector3.Lerp(initial, newScale, t));
        }

        public static Tween MoveTo(this Transform transform, Vector3 newPosition)
        {
            var initial = transform.localPosition;

            return initial == newPosition
                ? Tween.noop
                : new Tween(t => transform.localPosition = Vector3.Lerp(initial, newPosition, t));
        }

        public static Tween MoveBy(this Transform transform, Vector3 displacement)
        {
            var initial = transform.localPosition;
            var newPosition = initial + displacement; 

            return initial == newPosition
                ? Tween.noop
                : new Tween(t => transform.localPosition = Vector3.Lerp(initial, newPosition, t));
        }

        public static Tween RotateTo(this Transform transform, Quaternion newRotation)
        {
            var initial = transform.localRotation;
            var hasFailed = false;

            return new Tween(t => {
                if (hasFailed)
                    return;

                try {
                    transform.localRotation = Quaternion.Lerp(initial, newRotation, t);
                }
                catch {
                    // GPT 4 says quaternion equality check is unreliable, and that Unity does not allow lerping of quaternions
                    // that are very close together, for some reason.
                    Debug.LogWarning("Tween failed in RotateTo. Quaternions may be too close.");
                    hasFailed = true;
                    transform.localRotation = newRotation;
                }
            });
        }
    }
}
