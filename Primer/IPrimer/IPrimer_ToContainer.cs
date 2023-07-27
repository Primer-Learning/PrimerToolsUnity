using UnityEngine;

namespace Primer
{
    public static class IPrimer_ToContainerExtensions
    {
        public static Gnome ToContainer(this IPrimer self, bool connectToParent = false)
        {
            return self.transform.ToContainer(connectToParent);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Gnome ToContainer(this Transform self, bool connectToParent = false)
        {
            var container = new Gnome<Transform>(self);

            if (!connectToParent)
                return container;

            var parent = self.GetPrimer().parentGnome;

            if (parent is not null)
                parent.RegisterChildContainer(container);

            return container;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Gnome<T> ToContainer<T>(this T self, bool connectToParent = false) where T : Component
        {
            var container = new Gnome<T>(self);

            if (!connectToParent)
                return container;

            var parent = self.GetPrimer().parentGnome;

            if (parent is not null)
                parent.RegisterChildContainer(container);

            return container;
        }
    }
}
