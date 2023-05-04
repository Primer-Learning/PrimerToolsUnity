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

        public static Tween Series(params Tween[] tweenList)
        {
            var fullDuration = tweenList.Sum(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Series tween list is empty");
                return noop with { milliseconds = 0 };
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
