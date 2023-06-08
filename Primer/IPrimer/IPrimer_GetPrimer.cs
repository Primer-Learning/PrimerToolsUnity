using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_GetPrimerExtensions
    {
        public static IEnumerable<PrimerBehaviour> GetPrimer(this IEnumerable<IPrimer> self)
        {
            return self.GetOrAddComponent<PrimerBehaviour>();
        }

        public static IEnumerable<PrimerBehaviour> GetPrimer(this IEnumerable<Component> self)
        {
            return self.GetOrAddComponent<PrimerBehaviour>();
        }

        public static PrimerBehaviour GetPrimer(this IPrimer self)
        {
            return self.GetOrAddComponent<PrimerBehaviour>();
        }

        public static PrimerBehaviour GetPrimer(this Component self)
        {
            return self.GetOrAddComponent<PrimerBehaviour>();
        }
    }
}
