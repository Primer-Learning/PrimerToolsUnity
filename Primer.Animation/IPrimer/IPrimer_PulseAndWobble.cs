using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public static class IPrimer_PulseAndWobbleExtensions
    {
        public static Tween PulseAndWobble(
            this IPrimer self,
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

            var transform = self.component.transform;
            var pulse = self.Pulse(sizeFactor, attack, hold, decay);

            // State tracking vars
            var totalDuration = attack + hold + decay;
            var initialRotation = transform.localRotation;
            var quarterPeriod = wobbleCyclePeriod / 4; // Easier to work with

            var wobbleTweens = new List<Tween> {
                transform.RotateBy(z: -wobbleAngle) with {
                    duration = quarterPeriod,
                    easing = new QuadraticEasing(),
                },
            };

            var angleMultiplier = 2;

            for (
                totalDuration -= quarterPeriod;
                totalDuration >= 3 * quarterPeriod;
                totalDuration -= 2 * quarterPeriod
            ) {
                wobbleTweens.Add(
                    transform.RotateBy(z: wobbleAngle * angleMultiplier) with {
                        duration = 2 * quarterPeriod,
                        easing = new QuadraticEasing(),
                    }
                );

                angleMultiplier *= -1;
            }

            wobbleTweens.Add(
                transform.RotateTo(initialRotation) with {
                    duration = quarterPeriod,
                    easing = new QuadraticEasing(),
                }
            );

            return Tween.Parallel(
                Tween.Series(wobbleTweens.ToArray()),
                pulse
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
            // I refuse to copy the implementation of the method above so we use this shortcut to invoke it
            return self.ToPrimer().PulseAndWobble(sizeFactor, attack, hold, decay, wobbleAngle, wobbleCyclePeriod);
        }
    }
}
