using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public interface IPrimerPulse : ITransformHolder
    {
        Tween Pulse(float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f, float decay = 0.5f);
    }

    public static class CustomPrimerPulseExtensions
    {
        public static Tween PulseAndWobble(
            this IPrimerPulse self,
            float sizeFactor = 1.2f,
            float attack = 0.5f,
            float hold = 0.5f,
            float decay = 0.5f,
            float wobbleAngle = 5,
            float wobbleCyclePeriod = 0.75f)
        {
            return self.transform.PulseAndWobble(sizeFactor, attack, hold, decay, wobbleAngle, wobbleCyclePeriod);
        }

        public static Tween Pulse(this Component self, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f,
            float decay = 0.5f)
        {
            var transform = self.transform;
            var localScale = transform.localScale;

            return Tween.Series(
                transform.ScaleTo(localScale * sizeFactor) with { duration = attack },
                Tween.noop with { duration = hold },
                transform.ScaleTo(localScale, initialScale: localScale * sizeFactor) with { duration = decay }
            );
        }

        public static Tween PulseAndWobble(
            this Component self,
            float sizeFactor = 1.2f,
            float attack = 0.5f,
            float hold = 0.5f,
            float decay = 0.5f,
            float wobbleAngle = 5,
            float wobbleCyclePeriod = 0.75f)
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

            wobbleTweens.Add(
                transform.RotateTo(targetRotation) with { duration = quarterPeriod, easing = new QuadraticEasing() }
            );

            totalDuration -= quarterPeriod;
            currentRotation = targetRotation;

            var angleMultiplier = 1;

            while (totalDuration >= 3 * quarterPeriod)
            {
                targetRotation = Quaternion.Euler(0, 0, wobbleAngle * angleMultiplier) * initialRotation;

                wobbleTweens.Add(
                    transform.RotateTo(targetRotation, initialRotation: currentRotation) with
                    {
                        duration = 2 * quarterPeriod,
                        easing = new QuadraticEasing()
                    }
                );

                currentRotation = targetRotation;
                totalDuration -= 2 * quarterPeriod;
                angleMultiplier *= -1;
            }

            wobbleTweens.Add(
                transform.RotateTo(initialRotation, initialRotation: currentRotation) with
                {
                    duration = quarterPeriod,
                    easing = new QuadraticEasing()
                }
            );

            return Tween.Parallel(
                Tween.Series(wobbleTweens.ToArray()),
                pulse
            );
        }
    }
}
