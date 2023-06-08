using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_RemoveAllChildrenExtensions
    {
        public static void RemoveAllChildren(this IEnumerable<IPrimer> self)
        {
            foreach (var item in self)
                item.component.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this IEnumerable<Component> self)
        {
            foreach (var item in self)
                item.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this IPrimer self)
        {
            self.component.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this Component self)
        {
            self.transform.RemoveAllChildren();
        }

        public static void RemoveAllChildren(this Transform transform)
        {
            foreach (var child in transform.GetChildren())
                child.Dispose();
        }
    }
}
