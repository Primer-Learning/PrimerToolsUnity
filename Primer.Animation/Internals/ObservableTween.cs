using System;

namespace Primer.Animation
{
    public record ObservableTween(Action<float> lerp) : Tween(lerp)
    {
        public enum State { Idle, Playing, Completed }
        private State state = State.Idle;
        private bool isDisposed = false;

        public Action beforeStart { get; init; }
        public Action onStart { get; init; }
        public Action onComplete { get; init; }
        public Action afterComplete { get; init; }
        public Action onDispose { get; init; }
        public Action<float> onUpdate { get; init; }

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

        public override void Evaluate(float t, bool ignoreObserve = false)
        {
            if (delay is not 0 && t < tStart && !ignoreObserve)
                return;

            if (state != State.Playing && !ignoreObserve) {
                beforeStart?.Invoke();
                state = State.Playing;
            }

            if (t <= 0 && onStart is not null && !ignoreObserve) {
                onStart.Invoke();

                if (t == 0)
                {
                    base.Evaluate(t);
                    onUpdate?.Invoke(t);
                }
                
                return;
            }

            base.Evaluate(t);
            if (!ignoreObserve) onUpdate?.Invoke(t);
            
            if (t >= 1 && onComplete is not null && !ignoreObserve) {
                onComplete?.Invoke();

                if (state != State.Completed) {
                    afterComplete?.Invoke();
                    state = State.Completed;
                }

                return;
            }

            if (t >= 1 && !ignoreObserve) {
                afterComplete?.Invoke();
                state = State.Completed;
            }
        }
    }

    public static class ObservableTweenExtensions
    {
        public static ObservableTween Observe(
            this Tween tween,
            Action beforeStart = null,
            Action afterComplete = null,
            Action onStart = null,
            Action onComplete = null,
            Action onDispose = null,
            Action<float> afterUpdate = null)
        {
            if (tween is ObservableTween observableTween) {
                return ExtendObservable(
                    observableTween,
                    beforeStart,
                    afterComplete,
                    onStart,
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
                beforeStart = beforeStart,
                onStart = onStart,
                onUpdate = afterUpdate,
                onComplete = onComplete,
                afterComplete = afterComplete,
                onDispose = onDispose,
            };
        }

        private static ObservableTween ExtendObservable(
            ObservableTween tween,
            Action beforePlay,
            Action onComplete,
            Action whenZero,
            Action whenOne,
            Action onDispose,
            Action<float> afterUpdate)
        {
            return tween with {
                beforeStart = tween.beforeStart + beforePlay,
                onStart = tween.onStart + whenZero,
                onUpdate = tween.onUpdate + afterUpdate,
                onComplete = tween.onComplete + whenOne,
                afterComplete = tween.afterComplete + onComplete,
                onDispose = tween.onDispose + onDispose,
            };
        }
    }
}
