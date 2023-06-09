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
            wobbleTweens.Add(transform.RotateTo(targetRotation) with {duration = quarterPeriod, easing = new QuadraticEasing()});
            totalDuration -= quarterPeriod;
            currentRotation = targetRotation;

            var angleMultiplier = 1;
            while (totalDuration >= 3 * quarterPeriod)
            {
                targetRotation = Quaternion.Euler(0, 0, wobbleAngle * angleMultiplier) * initialRotation;
                wobbleTweens.Add(transform.RotateTo(targetRotation, initialRotation: currentRotation) with {duration = 2 * quarterPeriod, easing = new QuadraticEasing()});
                currentRotation = targetRotation;
                totalDuration -= 2 * quarterPeriod;
                angleMultiplier *= -1;
            }
            wobbleTweens.Add(transform.RotateTo(initialRotation, initialRotation: currentRotation) with {duration = quarterPeriod, easing = new QuadraticEasing()});

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
