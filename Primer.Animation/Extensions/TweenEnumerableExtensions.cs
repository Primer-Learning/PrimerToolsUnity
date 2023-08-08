using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public static class TweenEnumerableExtensions
    {
        public static IEnumerable<Tween> RemoveEmptyTweens(this IEnumerable<Tween> tweens)
        {
            return tweens.Where(x => x is not null && x != Tween.noop);
        }

        public static Tween RunInParallel(this IEnumerable<Tween> tweens)
        {
            return Tween.Parallel(tweens);
        }

        public static Tween RunInParallel(this IEnumerable<Tween> tweens, float delayBetweenStarts)
        {
            return Tween.Parallel(delayBetweenStarts, tweens.ToArray());
        }

        public static Tween RunInBatch(this IEnumerable<Tween> tweens)
        {
            var defaultEasing = IEasing.defaultMethod;
            var linear = LinearEasing.instance;
            var list = tweens.ToList();

            if (list.Any(x => x.easing != defaultEasing && x.easing != linear))
                Debug.LogWarning("Merging tweens with different easing methods. This is not recommended.");

            return list
                .Select(x => x with { easing = linear })
                .RunInParallel()
                with { easing = defaultEasing };
        }
    }
}
