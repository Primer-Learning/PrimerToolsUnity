using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;

namespace Primer.Timeline
{
    public static class Container_ScaleChildrenInPlayMode
    {
        public static T ScaleChildrenInPlayMode<T>(this T self, float duration = 0.5f) where T : Container
        {
            self.onCreate = x => (x.ScaleUpFromZero() with { duration = duration }).PlayAndForget();
            self.onRemove = (x, defer) => x.ShrinkAndDispose(duration, defer).Forget();
            return self;
        }

        public static T ScaleGrandchildrenInPlayMode<T>(this T self, float duration = 0.5f) where T : Container
        {
            self.onCreate = child => {
                child.GetChildren()
                    .Select(x => x.ScaleUpFromZero() with { duration = duration })
                    .RunInParallel()
                    .PlayAndForget();
            };

            self.onRemove = async (tick, defer) => {
                var childrenRemoval = tick
                    .GetChildren()
                    .Select(x => x.ShrinkAndDispose(duration));

                await UniTask.WhenAll(childrenRemoval);
                tick.Dispose();
            };

            return self;
        }
    }
}
