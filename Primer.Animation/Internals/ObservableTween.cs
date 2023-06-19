using System;

namespace Primer.Animation
{
    public record ObservableTween(Action<float> lerp) : Tween(lerp)
    {
        public enum State { Idle, Playing, Completed }
        private State state = State.Idle;
        private bool isDisposed = false;

        public Action whenZero { get; init; }
        public Action whenOne { get; init; }
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

            if (state != State.Playing) {
                beforePlay?.Invoke();
                state = State.Playing;
            }

            if (t <= 0 && whenZero is not null) {
                whenZero.Invoke();
                return;
            }

            if (t >= 1 && whenOne is not null) {
                whenOne?.Invoke();

                if (state != State.Completed) {
                    onComplete?.Invoke();
                    state = State.Completed;
                }

                return;
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
            Action beforePlay = null,
            Action onComplete = null,
            Action whenZero = null,
            Action whenOne = null,
            Action onDispose = null,
            Action<float> afterUpdate = null)
        {
            if (tween is ObservableTween observableTween) {
                return ExtendObservable(
                    observableTween,
                    beforePlay,
                    onComplete,
                    whenZero,
                    whenOne,
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
                whenZero = whenZero,
                whenOne = whenOne,
                beforePlay = beforePlay,
                onComplete = onComplete,
                onDispose = onDispose,
                afterUpdate = afterUpdate,
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
                whenZero = tween.whenZero + whenZero,
                whenOne = tween.whenOne + whenOne,
                beforePlay = tween.beforePlay + beforePlay,
                onComplete = tween.onComplete + onComplete,
                onDispose = tween.onDispose + onDispose,
                afterUpdate = tween.afterUpdate + afterUpdate,
            };
        }
    }
}
