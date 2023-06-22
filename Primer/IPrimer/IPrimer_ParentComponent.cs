using UnityEngine;

namespace Primer
{
    public static class IPrimer_ParentComponent
    {
        public static T ParentComponent<T>(this IPrimer self) where T : Component
        {
            return self.component.GetComponentInParent<T>();
        }

        public static T[] ParentComponents<T>(this IPrimer self) where T : Component
        {
            return self.component.GetComponentsInParent<T>();
        }

        public static T ParentComponent<T>(this IPrimer self, ref T cache) where T : Component
        {
            if (cache == null)
                cache = self.component.GetComponentInParent<T>();

            return cache;
        }

        public static T[] ParentComponents<T>(this IPrimer self, ref T[] cache) where T : Component
        {
            if (cache is null || cache.Length is 0)
                cache = self.component.GetComponentsInParent<T>();

            return cache;
        }

        public static T ParentComponent<T>(this Component self) where T : Component
        {
            return self.GetComponentInParent<T>();
        }

        public static T[] ParentComponents<T>(this Component self) where T : Component
        {
            return self.GetComponentsInParent<T>();
        }

        public static T ParentComponent<T>(this Component self, ref T cache) where T : Component
        {
            if (cache == null)
                cache = self.GetComponentInParent<T>();

            return cache;
        }

        public static T[] ParentComponents<T>(this Component self, ref T[] cache) where T : Component
        {
            if (cache is null || cache.Length is 0)
                cache = self.GetComponentsInParent<T>();

            return cache;
        }
    }
}
