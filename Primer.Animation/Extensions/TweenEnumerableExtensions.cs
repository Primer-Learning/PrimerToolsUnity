using System.Collections.Generic;
using System.Linq;

namespace Primer.Animation
{
    public static class TweenEnumerableExtensions
    {
        public static Tween RunInParallel(this IEnumerable<Tween> tweens)
        {
            return Tween.Parallel(tweens);
        }

        public static Tween RunInParallel(this IEnumerable<Tween> tweens, float delayBetweenStarts)
        {
            return Tween.Parallel(delayBetweenStarts, tweens.ToArray());
        }
    }
}
