using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_GetIntrinsicScaleExtensions
    {
        public static IEnumerable<Vector3> GetIntrinsicScale(this IEnumerable<IPrimer> self)
        {
            return self.Select(x => x.GetIntrinsicScale());
        }

        public static IEnumerable<Vector3> GetIntrinsicScale(this IEnumerable<Component> self)
        {
            return self.Select(x => x.GetIntrinsicScale());
        }

        public static Vector3 GetIntrinsicScale(this IPrimer self)
        {
            return self.GetPrimer().FindIntrinsicScale();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Vector3 GetIntrinsicScale(this Component self)
        {
            return self.GetPrimer().FindIntrinsicScale();
        }
    }
}
