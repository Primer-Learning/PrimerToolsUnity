using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_SetActiveExtensions
    {
        public static void SetActive(this IEnumerable<IPrimer> self, bool isActive)
        {
            foreach (var item in self)
                item.transform.gameObject.SetActive(isActive);
        }

        public static void SetActive(this IEnumerable<Component> self, bool isActive)
        {
            foreach (var item in self)
                item.gameObject.SetActive(isActive);
        }

        public static void SetActive(this IPrimer self, bool isActive)
        {
            self.transform.gameObject.SetActive(isActive);
        }

        public static void SetActive(this Component self, bool isActive)
        {
            self.gameObject.SetActive(isActive);
        }
    }
}
