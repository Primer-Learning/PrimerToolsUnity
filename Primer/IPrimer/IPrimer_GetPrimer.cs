using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_GetPrimerExtensions
    {
        public static IEnumerable<PrimerComponent> GetPrimer(this IEnumerable<IPrimer> self)
        {
            return self.GetOrAddComponent<PrimerComponent>();
        }

        public static IEnumerable<PrimerComponent> GetPrimer(this IEnumerable<Component> self)
        {
            return self.GetOrAddComponent<PrimerComponent>();
        }

        public static PrimerComponent GetPrimer(this IPrimer self)
        {
            return self.GetOrAddComponent<PrimerComponent>();
        }

        public static PrimerComponent GetPrimer(this Component self)
        {
            return self.GetOrAddComponent<PrimerComponent>();
        }
    }
}
