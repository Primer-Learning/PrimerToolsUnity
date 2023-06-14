using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_RemoveAllChildrenExtensions
    {
        public static void RemoveAllChildren(this IEnumerable<IPrimer> self)
        {
            foreach (var item in self)
                item.transform.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this IEnumerable<Component> self)
        {
            foreach (var item in self)
                item.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this IPrimer self)
        {
            self.transform.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this Component self)
        {
            self.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this Transform self)
        {
            foreach (var child in self.GetChildren())
                child.Dispose();
        }
    }
}
