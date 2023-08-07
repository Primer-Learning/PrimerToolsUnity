using Primer.Animation;

namespace Primer.Graph
{
    public partial class Axis : IAxisController
    {
        private Axis[] axesCache;
        public Axis[] axes => axesCache ??= new[] { this };


        public Tween GrowFromOrigin() => GrowFromOrigin(min, max);
        public Tween GrowFromOrigin(float newMax) => GrowFromOrigin(min, newMax);
        public Tween GrowFromOrigin(float newMin, float newMax)
        {
            range = new MinMax(min: 0, max: 0);
            UpdateChildren();

            range = new MinMax(min: newMin, max: newMax);
            this.SetActive(true);

            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                this.ScaleTo(1, initialScale: 0),
                Transition()
            );
        }

        public Tween ShrinkToOrigin()
        {
            range = new MinMax(min: 0, max: 0);

            return Tween.Parallel(
                delayBetweenStarts: 0.1f,
                Transition(),
                this.ScaleTo(0, initialScale: 1)
            );
        }

        public Tween SetScale(float newScale)
        {
            scale = newScale;
            return Transition();
        }

        public Tween SetDomain(float newMax, float newMin = 0)
        {
            range = new MinMax(newMin, newMax);
            return Transition();
        }

        public Tween Grow(float amount = 1, float amountNegative = 0)
        {
            min -= amountNegative;
            max += amount;
            return Transition();
        }

        public Tween GrowInSameSpace(float amount = 1, float amountNegative = 0)
        {
            var newMax = max + amount;
            var newMin = min - amountNegative;
            scale *= (max - min) / (newMax - newMin);
            range = new MinMax(newMin, newMax);
            return Transition();
        }
    }
}
