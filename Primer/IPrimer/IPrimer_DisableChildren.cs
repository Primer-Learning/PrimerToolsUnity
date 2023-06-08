using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_DisableChildrenExtensions
    {
        public static void DisableChildren(this IEnumerable<IPrimer> self)
        {
            foreach (var item in self)
                item.component.transform.DisableChildren();
        }

        public static void DisableChildren(this IEnumerable<Component> self)
        {
            foreach (var item in self)
                item.transform.DisableChildren();
        }

        public static void DisableChildren(this IPrimer self)
        {
            self.component.transform.DisableChildren();
        }

        public static void DisableChildren(this Component self)
        {
            self.transform.DisableChildren();
        }

        public static void DisableChildren(this Transform self)
        {
            foreach (var child in self.GetChildren()) {
                child.SetScale(0);
                child.SetActive(false);
            }
        }
    }
}
