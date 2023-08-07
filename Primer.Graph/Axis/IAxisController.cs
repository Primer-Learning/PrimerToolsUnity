using System.Linq;
using Primer.Animation;

namespace Primer.Graph
{
    public interface IAxisController
    {
        public Axis[] axes { get; }
    }

    public static class IAxisControllerExtensions
    {

        public static Tween SetScale(this IAxisController self, float newScale)
        {
            return self.axes.Select(axis => axis.SetScale(newScale)).RunInParallel();
        }

        public static Tween SetDomain(this IAxisController self, float newMax, float newMin = 0)
        {
            return self.axes.Select(axis => axis.SetDomain(newMax, newMin)).RunInParallel();
        }

        public static Tween GrowFromOrigin(this IAxisController self)
        {
            return self.axes.Select(axis => axis.GrowFromOrigin()).RunInParallel();
        }

        public static Tween GrowFromOrigin(this IAxisController self, float newMax)
        {
            return self.axes.Select(axis => axis.GrowFromOrigin(newMax)).RunInParallel();
        }

        public static Tween GrowFromOrigin(this IAxisController self, float newMin, float newMax)
        {
            return self.axes.Select(axis => axis.GrowFromOrigin(newMax)).RunInParallel();
        }

        public static Tween ShrinkToOrigin(this IAxisController self)
        {
            return self.axes.Select(axis => axis.ShrinkToOrigin()).RunInParallel();
        }

        public static Tween Grow(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.Grow(amount, amountNegative)).RunInParallel();
        }

        public static Tween Shrink(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.Shrink(amount, amountNegative)).RunInParallel();
        }

        public static Tween GrowInSameSpace(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.GrowInSameSpace(amount, amountNegative)).RunInParallel();
        }
    }
}
