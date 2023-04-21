using UnityEngine;

namespace Primer.Animation
{
    public static class TransformExtensions
    {
        public static Tween MoveAndRotate(this Transform transform, Vector3 newPosition, Quaternion newRotation)
        {
            return Tween.Parallel(
                transform.MoveTo(newPosition),
                transform.RotateTo(newRotation)
            );
        }

        // Convenience overload method for scaling to multiples of Vector3.one
        public static Tween ScaleTo(this Transform transform, float newScale)
        {
            return transform.ScaleTo(newScale * Vector3.one);
        }

        public static Tween ScaleTo(this Transform transform, Vector3 newScale)
        {
            var initial = transform.localScale;

            return new Tween(t => transform.localScale = Vector3.Lerp(initial, newScale, t));
            
            // No longer doing it this way, because it breaks the Pulse method, which has Tweens in series that
            // change and then restore the scale value. I don't understand the internals enough to quite understand why
            // but it goes something like "the first tween hasn't executed when the second one is defined, so the second
            // one doesn't realize initial and newScale will actually be different when it's time for it to execute.
            // return initial == newScale
            //     ? Tween.noop
            //     : new Tween(t => transform.localScale = Vector3.Lerp(initial, newScale, t));
        }

        public static Tween Pulse(this Transform transform, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f, float decay = 0.5f)
        {
            var localScale = transform.localScale;
            return Tween.Series(
                transform.ScaleTo(localScale * sizeFactor) with {duration = attack},
                Tween.noop with {duration = hold},
                transform.ScaleTo(localScale) with {duration = decay}
            );
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
