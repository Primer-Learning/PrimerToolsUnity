using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween
    {
        public static Tween Parallel(float delayBetweenStarts, params Tween[] tweenList)
        {
            var tweens = tweenList
                .RemoveEmptyTweens()
                .Select((tween, i) => tween with { delay = delayBetweenStarts * i })
                .ToArray();

            return Parallel_Internal(tweens);
        }

        public static Tween Parallel(IEnumerable<Tween> tweenList)
        {
            return Parallel_Internal(tweenList.RemoveEmptyTweens().ToArray());
        }

        public static Tween Parallel(params Tween[] tweenList)
        {
            return Parallel_Internal(tweenList.RemoveEmptyTweens().ToArray());
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private static Tween Parallel_Internal(Tween[] tweenList)
        {
            if (tweenList.Length is 0)
                return noop;

            if (tweenList.Length is 1)
                return tweenList[0];

            var fullDuration = tweenList.Max(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Parallel tween list is empty");
                return noop;
            }

            var result = new Tween(
                t => {
                    for (var i = 0; i < tweenList.Length; i++) {
                        var tween = tweenList[i];
                        tween.Evaluate(Mathf.Clamp01(t / tween.totalDuration * fullDuration));
                    }
                }
            ) {
                easing = LinearEasing.instance,
                duration = fullDuration,
                isCalculated = true,
            };

            var observed = tweenList
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
