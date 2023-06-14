using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_GetPrimerExtensions
    {
        public static IEnumerable<PrimerComponent> GetPrimer(this IEnumerable<IPrimer> self)
        {
            return self.Select(x => x.GetPrimer());
        }

        public static IEnumerable<PrimerComponent> GetPrimer(this IEnumerable<Component> self)
        {
            return self.Select(x => x.GetPrimer());
        }

        public static PrimerComponent GetPrimer(this IPrimer self)
        {
            return self as PrimerComponent ?? self.GetOrAddComponent<PrimerComponent>();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static PrimerComponent GetPrimer(this Component self)
        {
            return self as PrimerComponent ?? self.GetOrAddComponent<PrimerComponent>();
        }
    }
}
