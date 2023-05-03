using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween
    {
        public static Tween Parallel(float delayBetweenStarts, params Tween[] tweenList)
        {
            return Parallel(tweenList.Select((tween, i) => tween with { delay = delayBetweenStarts * i }));
        }

        public static Tween Parallel(IEnumerable<Tween> tweenList)
        {
            return Parallel(tweenList.ToArray());
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Tween Parallel(params Tween[] tweenList)
        {
            if (tweenList.Length == 0)
                return noop with { milliseconds = 0 };

            var fullDuration = tweenList.Max(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Parallel tween list is empty");
                return noop with { milliseconds = 0 };
            }

            return new Tween(
                t => {
                    for (var i = 0; i < tweenList.Length; i++) {
                        var tween = tweenList[i];
                        tween.Evaluate(Mathf.Clamp01(t / tween.totalDuration * fullDuration));
                    }
                }
            ) {
                easeMethod = LinearEasing.instance,
                duration = fullDuration,
                isCalculated = true,
            };
        }
    }
}
