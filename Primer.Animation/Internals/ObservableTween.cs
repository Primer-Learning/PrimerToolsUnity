using System;

namespace Primer.Animation
{
    public record ObservableTween(Action<float> lerp) : Tween(lerp)
    {
        public enum State { Idle, Playing, Completed }
        private State state = State.Idle;

        public Action beforePlay { get; init; }
        public Action onComplete { get; init; }
        // public Action<float> beforeUpdate { get; init; }
        public Action<float> afterUpdate { get; init; }

        public override void Evaluate(float t)
        {
            if (state != State.Playing && t > tStart) {
                beforePlay?.Invoke();
                state = State.Playing;
            }

            // beforeUpdate?.Invoke(t);
            base.Evaluate(t);
            afterUpdate?.Invoke(t);

            if (state == State.Playing && t >= 1) {
                onComplete?.Invoke();
                state = State.Completed;
            }
        }
    }

    public static class ObservableTweenExtensions
    {
        public static ObservableTween Observe(
            this Tween tween,
            Action beforePlay = null,
            Action onComplete = null,
            // Action<float> beforeUpdate = null,
            Action<float> afterUpdate = null
        ) {
            return new ObservableTween(tween.lerp) {
                // all of Tween fields should be copied here
                easeMethod = tween.easeMethod,
                delay = tween.delay,
                isCalculated = tween.isCalculated,
                ms = tween.ms,
                // listeners
                beforePlay = beforePlay,
                onComplete = onComplete,
                // beforeUpdate = beforeUpdate,
                afterUpdate = afterUpdate,
            };
        }
    }
}
