using UnityEngine;

namespace Primer
{
    public static class IPrimer_ChildComponent
    {
        public static T ChildComponent<T>(this IPrimer self) where T : Component
        {
            return self.component.GetComponentInChildren<T>();
        }

        public static T[] ChildComponents<T>(this IPrimer self) where T : Component
        {
            return self.component.GetComponentsInChildren<T>();
        }

        public static T ChildComponent<T>(this IPrimer self, ref T cache) where T : Component
        {
            if (cache == null)
                cache = self.component.GetComponentInChildren<T>();

            return cache;
        }

        public static T[] ChildComponents<T>(this IPrimer self, ref T[] cache) where T : Component
        {
            if (cache is null || cache.Length is 0)
                cache = self.component.GetComponentsInChildren<T>();

            return cache;
        }

        public static T ChildComponent<T>(this Component self) where T : Component
        {
            return self.GetComponentInChildren<T>();
        }

        public static T[] ChildComponents<T>(this Component self) where T : Component
        {
            return self.GetComponentsInChildren<T>();
        }

        public static T ChildComponent<T>(this Component self, ref T cache) where T : Component
        {
            if (cache == null)
                cache = self.GetComponentInChildren<T>();

            return cache;
        }

        public static T[] ChildComponents<T>(this Component self, ref T[] cache) where T : Component
        {
            if (cache is null || cache.Length is 0)
                cache = self.GetComponentsInChildren<T>();

            return cache;
        }
    }
}
