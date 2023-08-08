using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween
    {
        public static Tween Series(IEnumerable<Tween> tweenList)
        {
            return Series(tweenList.ToArray());
        }

        // This overload doesn't work because Tween.Series() needs to know how much time to allocate to each tween.
        // this is unknown until the function is called.
        // otherwise it won't know when to invoke the next tween in the list.
        //
        // In the following example, the first tween is 0.1 seconds long, and the second tween is 0.9 seconds long.
        // By looking at the source code we know the second function should be invoked when `t > 0.01`
        // but Tween.Series() has no way of knowing this.
        //
        // Tween.Series(
        //     () => SomeQuickTween() with { duration = 0.01f },
        //     () => SomeSlowTween() with { duration = 0.99f },
        // );
        //
        // public static Tween Series(params System.Func<Tween>[] tweenList)

        public static Tween Series(params Tween[] tweenList)
        {
            var fullDuration = tweenList.Sum(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Series tween list is empty");
                return noop;
            }

            var cursor = 0;
            var cursorEnd = tweenList[0].duration;
            var cursorStartT = 0f;
            var cursorEndT = cursorEnd / fullDuration;

            return new Tween(
                t => {
                    while (t > cursorEndT) {
                        tweenList[cursor].Evaluate(1);
                        cursor++;
                        tweenList[cursor].Evaluate(0);

                        var cursorStart = cursorEnd;
                        cursorEnd += tweenList[cursor].duration;
                        cursorStartT = cursorStart / fullDuration;
                        cursorEndT = cursorEnd / fullDuration;
                    }

                    var tweenT = PrimerMath.Remap(cursorStartT, cursorEndT, 0, 1, t);
                    tweenList[cursor].Evaluate(tweenT);
                }
            ) {
                easing = LinearEasing.instance,
                duration = fullDuration,
                isCalculated = true,
            };
        }
    }
}
