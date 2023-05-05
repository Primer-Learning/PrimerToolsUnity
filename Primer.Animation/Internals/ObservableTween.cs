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
            if (delay is not 0 && t < tStart)
                return;

            if (state != State.Playing) {
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
            Action<float> afterUpdate = null)
        {
            if (tween is ObservableTween observableTween)
                return ExtendObservable(observableTween, beforePlay, onComplete, afterUpdate);

            return new ObservableTween(tween.lerp) {
                // all of Tween fields should be copied here
                easing = tween.easing,
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

        private static ObservableTween ExtendObservable(
            ObservableTween tween,
            Action beforePlay,
            Action onComplete,
            Action<float> afterUpdate)
        {
            return tween with {
                beforePlay = Merge(tween.beforePlay, beforePlay),
                onComplete = Merge(tween.onComplete, onComplete),
                afterUpdate = Merge(tween.afterUpdate, afterUpdate),
            };
        }

        private static Action Merge(Action a, Action b)
        {
            if (a is null || b is null)
                return a ?? b;

            return () => {
                a.Invoke();
                b.Invoke();
            };
        }

        private static Action<T> Merge<T>(Action<T> a, Action<T> b)
        {
            if (a is null || b is null)
                return a ?? b;

            return x => {
                a.Invoke(x);
                b.Invoke(x);
            };
        }

    }
}
