using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_RemoveAllChildrenExtensions
    {
        public static void RemoveAllChildren(this IEnumerable<IPrimer> self, bool defer = false)
        {
            foreach (var item in self)
                item.transform.transform.RemoveAllChildren(defer);
        }

        public static void RemoveAllChildren(this IEnumerable<Component> self, bool defer = false)
        {
            foreach (var item in self)
                item.transform.RemoveAllChildren(defer);
        }

        public static void RemoveAllChildren(this IPrimer self, bool defer = false)
        {
            self.transform.transform.RemoveAllChildren(defer);
        }

        public static void RemoveAllChildren(this Component self, bool defer = false)
        {
            self.transform.RemoveAllChildren(defer);
        }

        public static void RemoveAllChildren(this Transform self, bool defer = false)
        {
            foreach (var child in self.GetChildren())
                child.Dispose(defer);
        }
    }
}
