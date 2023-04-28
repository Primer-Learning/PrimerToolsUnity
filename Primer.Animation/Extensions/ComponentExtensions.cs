using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public static class ComponentExtensions
    {
        public static Tween MoveAndRotate(this Component self, Vector3 newPosition, Quaternion newRotation)
        {
            return Tween.Parallel(
                self.MoveTo(newPosition),
                self.RotateTo(newRotation)
            );
        }

        public static Tween ScaleUpFromZero(this Component self)
        {
            return self.GetPrimer().ScaleUpFromZero();
        }

        public static Tween ScaleTo(this Component self, float newScale)
        {
            return self.ScaleTo(newScale * Vector3.one);
        }

        public static Tween ScaleTo(this Component self, Vector3 newScale, Vector3? initialScale = null)
        {
            var transform = self.transform;
            var initial = initialScale ?? transform.localScale;

            return initial == newScale
                ? Tween.noop
                : new Tween(t => transform.localScale = Vector3.Lerp(initial, newScale, t));
        }

        public static Tween Pulse(this Component self, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f, float decay = 0.5f)
        {
            var transform = self.transform;
            var localScale = transform.localScale;

            return Tween.Series(
                transform.ScaleTo(localScale * sizeFactor) with {duration = attack},
                Tween.noop with {duration = hold},
                transform.ScaleTo(localScale, initialScale: localScale * sizeFactor) with {duration = decay}
            );
        }

        public static Tween PulseAndWobble(
            this Component self,
            float sizeFactor = 1.2f,
            float attack = 0.5f,
            float hold = 0.5f,
            float decay = 0.5f,
            float wobbleAngle = 5,
            float wobbleCyclePeriod = 0.75f
            )
        {
            // The defaults lead to two full wobble cycles over the duration of the pulse
            // Quadratic easing is used for the wobbles since that reflects the behavior of simple harmonic oscillators,
            // making it look more natural.

            var transform = self.transform;
            var pulse = transform.Pulse(sizeFactor, attack, hold, decay);

            // State tracking vars
            var initialRotation = transform.localRotation;
            var totalDuration = attack + hold + decay;
            // Need to keep track of this and feed to the RotateToCalls as the initial rotation,
            // since the Tweens called in series don't know about each other.
            var currentRotation = initialRotation;

            var quarterPeriod = wobbleCyclePeriod / 4; // Easier to work with
            var wobbleTweens = new List<Tween>();
            var targetRotation = Quaternion.Euler(0, 0, -wobbleAngle) * initialRotation;
            wobbleTweens.Add(transform.RotateTo(targetRotation) with {duration = quarterPeriod, easeMethod = new QuadraticEasing()});
            totalDuration -= quarterPeriod;
            currentRotation = targetRotation;

            var angleMultiplier = 1;
            while (totalDuration >= 3 * quarterPeriod)
            {
                targetRotation = Quaternion.Euler(0, 0, wobbleAngle * angleMultiplier) * initialRotation;
                wobbleTweens.Add(transform.RotateTo(targetRotation, initialRotation: currentRotation) with {duration = 2 * quarterPeriod, easeMethod = new QuadraticEasing()});
                currentRotation = targetRotation;
                totalDuration -= 2 * quarterPeriod;
                angleMultiplier *= -1;
            }
            wobbleTweens.Add(transform.RotateTo(initialRotation, initialRotation: currentRotation) with {duration = quarterPeriod, easeMethod = new QuadraticEasing()});

            return Tween.Parallel(
                Tween.Series(wobbleTweens.ToArray()),
                pulse
            );
        }

        public static Tween MoveTo(this Component self, Vector3 newPosition)
        {
            var transform = self.transform;
            var initial = transform.localPosition;

            return initial == newPosition
                ? Tween.noop
                : new Tween(t => transform.localPosition = Vector3.Lerp(initial, newPosition, t));
        }

        public static Tween MoveBy(this Component self, Vector3 displacement)
        {
            var transform = self.transform;
            var initial = transform.localPosition;
            var newPosition = initial + displacement;

            return initial == newPosition
                ? Tween.noop
                : new Tween(t => transform.localPosition = Vector3.Lerp(initial, newPosition, t));
        }

        public static Tween RotateTo(this Component self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var transform = self.transform;
            var initial = initialRotation ?? transform.localRotation;
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
