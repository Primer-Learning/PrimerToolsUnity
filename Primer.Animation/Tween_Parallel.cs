using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween
    {
        public static Tween Parallel(float delayBetweenStarts, params Tween[] tweenArray)
        {
            var tweens = tweenArray
                .RemoveEmptyTweens()
                .Select((tween, i) => tween with { delay = delayBetweenStarts * i })
                .ToArray();

            return Parallel_Internal(tweens);
        }
        public static Tween Parallel(float totalDuration, float durationPerIndividualTween, params Tween[] tweenArray)
        {
            if (tweenArray.Length is 0)
                return tweenArray[0] with { duration = totalDuration };
            var delayBetweenStarts = (totalDuration - durationPerIndividualTween) / (tweenArray.Length - 1);
            return Parallel(delayBetweenStarts, tweenArray.Select(x => x with { duration = durationPerIndividualTween }).ToArray());
        }

        public static Tween Parallel(IEnumerable<Tween> tweenArray)
        {
            return Parallel_Internal(tweenArray.RemoveEmptyTweens().ToArray());
        }

        public static Tween Parallel(params Tween[] tweenArray)
        {
            return Parallel_Internal(tweenArray.RemoveEmptyTweens().ToArray());
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private static Tween Parallel_Internal(Tween[] tweenArray)
        {
            if (tweenArray.Length is 0)
                return noop;

            if (tweenArray.Length is 1)
                return tweenArray[0];

            var fullDuration = tweenArray.Max(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Parallel tween list is empty");
                return noop;
            }

            var result = new Tween(
                t => {
                    for (var i = 0; i < tweenArray.Length; i++) {
                        var tween = tweenArray[i];
                        tween.Evaluate(Mathf.Clamp01(t / tween.totalDuration * fullDuration));
                    }
                }
            ) {
                easing = LinearEasing.instance,
                duration = fullDuration,
                isCalculated = true,
            };

            var observed = tweenArray
                .Select(x => x as ObservableTween)
                .Where(x => x?.onDispose != null)
                .ToList();

            if (observed.Count is 0)
                return result;

            return result.Observe(
                onDispose: () => {
                    foreach (var tween in observed)
                        tween.onDispose();
                }
            );
        }
    }
}
