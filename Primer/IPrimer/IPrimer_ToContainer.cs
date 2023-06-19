using UnityEngine;

namespace Primer
{
    public static class IPrimer_ToContainerExtensions
    {
        public static Container ToContainer(this IPrimer self, bool connectToParent = false)
        {
            return self.transform.ToContainer(connectToParent);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Container ToContainer(this Transform self, bool connectToParent = false)
        {
            var container = new Container<Transform>(self);

            if (!connectToParent)
                return container;

            var parent = self.GetPrimer().parentContainer;

            if (parent is not null)
                parent.RegisterChildContainer(container);

            return container;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Container<T> ToContainer<T>(this T self, bool connectToParent = false) where T : Component
        {
            var container = new Container<T>(self);

            if (!connectToParent)
                return container;

            var parent = self.GetPrimer().parentContainer;

            if (parent is not null)
                parent.RegisterChildContainer(container);

            return container;
        }
    }
}
