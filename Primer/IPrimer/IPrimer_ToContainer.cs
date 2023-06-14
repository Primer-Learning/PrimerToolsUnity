using UnityEngine;

namespace Primer
{
    public static class IPrimer_ToContainerExtensions
    {
        public static Container ToContainer(this IPrimer self)
        {
            return self.component.transform.ToContainer();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Container ToContainer(this Transform self)
        {
            var container = new Container(self);
            var parent = self.GetPrimer().parentContainer;

            if (parent is not null)
                parent.RegisterChildContainer(container);

            return container;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static Container<T> ToContainer<T>(this T self) where T : Component
        {
            var container = new Container<T>(self);
            var parent = self.GetPrimer().parentContainer;

            if (parent is not null)
                parent.RegisterChildContainer(container);

            return container;
        }
    }
}
