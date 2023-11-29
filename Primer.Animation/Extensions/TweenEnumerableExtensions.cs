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
        public static Tween RunInParallel(this IEnumerable<Tween> tween, float totalDuration, float durationPerIndividualTween)
        {
            return Tween.Parallel(totalDuration, durationPerIndividualTween, tween.ToArray());
        }

        // This is meant to allow easing to be defined for the whole ensemble rather than each individual tween.
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
        
        #region IEnumerable<IEnumerable<Tween>> Parallelize
        public static IEnumerable<Tween> ParallelizeTweenLists(this IEnumerable<IEnumerable<Tween>> listOfListsOfTweens)
        {
            var enumerators = listOfListsOfTweens.Select(e => e.GetEnumerator())
                .ToList();

            var moreTweens = enumerators.Any();

            while (moreTweens)
            {
                var parallelTweens = new List<Tween>();
                moreTweens = false;
                foreach (var enumerator in enumerators)
                {
                    if (enumerator.MoveNext())
                    {
                        parallelTweens.Add(enumerator.Current);
                    }
                }
                if (parallelTweens.Any())
                {
                    moreTweens = true;
                    yield return parallelTweens.RunInParallel();
                }
            }
            // Dispose of the enumerators
            foreach (var enumerator in enumerators)
            {
                enumerator.Dispose();
            }
        }
        
        #endregion
    }
}
