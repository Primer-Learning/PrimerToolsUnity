using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public interface IPrimer_CustomPulse
    {
        Tween Pulse(float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f, float decay = 0.5f);
    }

    public static class IPrimer_PulseExtensions
    {
        public static Tween Pulse(this IPrimer self, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f,
            float decay = 0.5f)
        {
            var transform = self.transform;
            var localScale = transform.localScale;

            return Tween.Series(
                self.ScaleTo(localScale * sizeFactor) with { duration = attack },
                Tween.noop with { duration = hold },
                self.ScaleTo(localScale) with { duration = decay }
            );
        }

        // This is a copy of the method above with
        // - Component instead of IPrimer
        public static Tween Pulse(this Component self, float sizeFactor = 1.2f, float attack = 0.5f, float hold = 0.5f,
            float decay = 0.5f)
        {
            var transform = self.transform;
            var localScale = transform.localScale;
            var pulseSize = localScale * sizeFactor;

            return Tween.Series(
                self.ScaleTo(pulseSize) with { duration = attack },
                Tween.noop with { duration = hold },

                // If we don't set initialScale, the initial value will be the scale now
                // before the previous scaling is actually applied
                self.ScaleTo(localScale, initialScale: pulseSize) with { duration = decay }
            );
        }

        public static Tween PulseGroup(this IEnumerable<IPrimer> objectsToPulse, float sizeFactor = 1.2f,
            float attack = 0.5f, float hold = 0.5f, float decay = 0.5f)
        {
            var list = objectsToPulse.ToList();

            // Calculate the center of the group in world/global space
            var center = list.Select(x => x.transform.position).Average();

            return list.Select(
                    primer => {
                        var transform = primer.transform;
                        var localScale = transform.localScale;
                        var originalPosition = transform.position;
                        var direction = (transform.position - center).normalized;
                        var distance = Vector3.Distance(originalPosition, center) * (sizeFactor - 1);

                        // Create the Tweens and add them to the list
                        return Tween.Series(
                            Tween.Parallel(
                                primer.ScaleTo(localScale * sizeFactor) with { duration = attack },
                                primer.MoveTo(originalPosition + direction * distance, globalSpace: true) with {
                                    duration = attack,
                                }
                            ),
                            Tween.noop with { duration = hold },
                            Tween.Parallel(
                                primer.ScaleTo(localScale, initialScale: localScale * sizeFactor) with {
                                    duration = decay,
                                },
                                primer.MoveTo(
                                    originalPosition,
                                    initialPosition: originalPosition + direction * distance,
                                    globalSpace: true
                                ) with { duration = decay }
                            )
                        );

                    }
                )
                .RunInParallel();
        }

        public static Tween PulseGroup(this IEnumerable<Transform> objectsToPulse, float sizeFactor = 1.2f,
            float attack = 0.5f, float hold = 0.5f, float decay = 0.5f)
        {
            return objectsToPulse.Select(x => x.ToPrimer()).PulseGroup(sizeFactor, attack, hold, decay);
        }
    }
}
