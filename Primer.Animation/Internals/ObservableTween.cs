using System;

namespace Primer.Animation
{
    public record ObservableTween(Action<float> lerp) : Tween(lerp)
    {
        public enum State { Idle, Playing, Completed }
        private State state = State.Idle;
        private bool isDisposed = false;

        public Action onInitialState { get; init; }
        public Action beforePlay { get; init; }
        public Action onComplete { get; init; }
        public Action onDispose { get; init; }
        public Action<float> afterUpdate { get; init; }

        ~ObservableTween()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            base.Dispose();
            onDispose?.Invoke();
        }

        public override void Evaluate(float t)
        {
            if (delay is not 0 && t < tStart)
                return;

            if (t <= 0 && onInitialState is not null) {
                onInitialState.Invoke();
                state = State.Idle;
                return;
            }

            if (state != State.Playing) {
                beforePlay?.Invoke();
                state = State.Playing;
            }

            base.Evaluate(t);
            afterUpdate?.Invoke(t);

            if (t >= 1) {
                onComplete?.Invoke();
                state = State.Completed;
            }
        }
    }

    public static class ObservableTweenExtensions
    {
        public static ObservableTween Observe(
            this Tween tween,
            Action onInitialState = null,
            Action beforePlay = null,
            Action onComplete = null,
            Action onDispose = null,
            Action<float> afterUpdate = null)
        {
            if (tween is ObservableTween observableTween) {
                return ExtendObservable(
                    observableTween,
                    onInitialState,
                    beforePlay,
                    onComplete,
                    onDispose,
                    afterUpdate
                );
            }

            return new ObservableTween(tween.lerp) {
                // all of Tween fields should be copied here
                easing = tween.easing,
                delay = tween.delay,
                isCalculated = tween.isCalculated,
                ms = tween.ms,

                // listeners
                onInitialState = onInitialState,
                beforePlay = beforePlay,
                onComplete = onComplete,
                onDispose = onDispose,
                afterUpdate = afterUpdate,
            };
        }

        private static ObservableTween ExtendObservable(
            ObservableTween tween,
            Action onInitialState,
            Action beforePlay,
            Action onComplete,
            Action onDispose,
            Action<float> afterUpdate)
        {
            return tween with {
                onInitialState = tween.onInitialState + onInitialState,
                beforePlay = tween.beforePlay + beforePlay,
                onComplete = tween.onComplete + onComplete,
                onDispose = tween.onDispose + onDispose,
                afterUpdate = tween.afterUpdate + afterUpdate,
            };
        }
    }
}
