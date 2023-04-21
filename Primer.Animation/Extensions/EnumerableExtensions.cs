using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Animation
{
    public static class EnumerableExtensions
    {
        public static Tween ScaleDownToZero(this IEnumerable<Transform> self)
        {
            return Tween.Parallel(self.Select(x => x.ScaleTo(0)));
        }

        public static Tween ScaleTo(this IEnumerable<Transform> self, float scale) => self.ScaleTo(Vector3.one * scale);
        public static Tween ScaleTo(this IEnumerable<Transform> self, Vector3 targetScale)
        {
            return Tween.Parallel(self.Select(x => x.ScaleTo(targetScale)));
        }
    }
}
