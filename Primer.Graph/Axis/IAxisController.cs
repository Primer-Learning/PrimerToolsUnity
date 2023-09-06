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

        public static Tween SetGraphScale(this IAxisController self, float newScale)
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

        public static Tween GrowFromOrigin(this IAxisController self, float newLength)
        {
            return self.axes.Select(axis => axis.GrowFromOrigin(newLength)).RunInParallel();
        }

        public static Tween ShrinkToOrigin(this IAxisController self)
        {
            return self.axes.Select(axis => axis.ShrinkToOrigin()).RunInParallel();
        }

        public static Tween GrowDomain(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.GrowDomain(amount, amountNegative)).RunInParallel();
        }

        public static Tween ShrinkDomain(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.GrowDomain(-amount, -amountNegative)).RunInParallel();
        }

        public static Tween GrowDomainInSameSpace(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.GrowDomainInSameSpace(amount, amountNegative)).RunInParallel();
        }


        public static Tween ShrinkDomainInSameSpace(this IAxisController self, float amount = 1, float amountNegative = 0)
        {
            return self.axes.Select(axis => axis.GrowDomainInSameSpace(-amount, -amountNegative)).RunInParallel();
        }
    }
}
