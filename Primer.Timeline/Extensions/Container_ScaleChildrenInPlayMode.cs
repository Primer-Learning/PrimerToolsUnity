using System.Linq;
using Cysharp.Threading.Tasks;
using Primer.Animation;

namespace Primer.Timeline
{
    public static class Container_ScaleChildrenInPlayMode
    {
        public static T ScaleChildrenInPlayMode<T>(this T self) where T : Container
        {
            self.onCreate = x => x.ScaleUpFromZero().PlayAndForget();
            self.onRemove = x => x.ShrinkAndDispose().Forget();
            return self;
        }

        public static T ScaleGrandchildrenInPlayMode<T>(this T self) where T : Container
        {
            self.onCreate = child => {
                child.GetChildren()
                    .Select(x => x.ScaleUpFromZero() with { duration = 0.5f })
                    .RunInParallel()
                    .PlayAndForget();
            };

            self.onRemove = async tick => {
                var childrenRemoval = tick
                    .GetChildren()
                    .Select(x => x.ShrinkAndDispose(0.1f));

                await UniTask.WhenAll(childrenRemoval);
                tick.Dispose();
            };

            return self;
        }
    }
}
