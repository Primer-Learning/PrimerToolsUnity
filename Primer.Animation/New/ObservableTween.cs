using System;

namespace Primer.Animation
{
    public record ObservableTween(Action<float> lerp) : Tween(lerp)
    {
        public enum State { Idle, Playing, Completed }
        private State state = State.Idle;

        public Action beforePlay { get; init; }
        public Action onComplete { get; init; }

        public override void Evaluate(float t)
        {
            if (state != State.Playing && t > tStart) {
                beforePlay?.Invoke();
                state = State.Playing;
            }

            base.Evaluate(t);

            if (state == State.Playing && t >= 1) {
                onComplete?.Invoke();
                state = State.Completed;
            }
        }
    }

    public static class ObservableTweenExtensions
    {
        public static ObservableTween Observe(this Tween tween, Action beforePlay = null, Action onComplete = null)
        {
            return new ObservableTween(tween.lerp) {
                // all of Tween fields should be copied here
                easeMethod = tween.easeMethod,
                delay = tween.delay,
                isCalculated = tween.isCalculated,
                ms = tween.ms,
                beforePlay = beforePlay,
                onComplete = onComplete,
            };
        }
    }
}
