using UnityEngine;

namespace Primer
{
    public class IPrimer_ToContainerExtensions
    {
        public static Container ToContainer(IPrimer self)
        {
            var transform = self.component.transform;
            var parent = self.GetPrimer().parentContainer;

            return parent is not null
                ? parent.WrapChild(transform)
                : new Container(transform);
        }

        public static Container<T> ToContainer<T>(T self) where T : Component
        {
            var parent = self.GetPrimer().parentContainer;

            return parent is not null
                ? parent.WrapChild(self)
                : new Container<T>(self);
        }
    }
}
