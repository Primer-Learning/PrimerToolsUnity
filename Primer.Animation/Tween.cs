using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween(Action<float> lerp) : IDisposable
    {
        public static Tween noop = new(_ => {});

        public IEasing easing { get; init; } = IEasing.defaultMethod;
        public EaseMode easeMode {
            get => EaseModeExtensions.GetModeFor(easing);
            init => easing = value.GetMethod();
        }

        public float delay = 0f;

        #region public float duration;
        internal bool isCalculated { get; init; } = false;
        internal int ms { get; init; } = 500;

        public int milliseconds {
            get => ms;
            init {
                if (isCalculated) {
                    Debug.LogWarning("Forcing the duration of a calculated tween");
                    isCalculated = false;
                }

                ms = value;
            }
        }

        public float seconds {
            get => milliseconds / 1000f;
            init => milliseconds = (int)(value * 1000);
        }

        public float duration {
            get => milliseconds / 1000f;
            init => milliseconds = (int)(value * 1000);
        }
        #endregion

        public float totalDuration => duration + delay;
        internal float tStart => 1 / totalDuration * delay;

        public virtual void Evaluate(float t)
        {
            if (delay is not 0) {
                if (t < tStart)
                    return;

                t = Mathf.Clamp01(PrimerMath.Remap(tStart, 1, 0, 1, t));
            }

            lerp(easing.Evaluate(t));
        }

        #region public void Play();
        public async void PlayAndForget(CancellationToken ct = default) => await Play(ct);

        public async UniTask Play(CancellationToken ct = default)
        {
            if (!Application.isPlaying) {
                Evaluate(1);
                return;
            }

            var startTime = Time.time;
            var delayAndDuration = totalDuration;

            if (TryEvaluate(0))
                return;

            while (!ct.IsCancellationRequested && Time.time < startTime + delayAndDuration) {
                var t = (Time.time - startTime) / delayAndDuration;

                if (TryEvaluate(t))
                    return;

                await UniTask.DelayFrame(1, cancellationToken: ct);

                if (ct.IsCancellationRequested)
                    return;
            }

            TryEvaluate(1);
        }

        private bool TryEvaluate(float t)
        {
            try {
                Evaluate(t);
            }
            catch (MissingReferenceException) {
                // The object we're tweening has been destroyed
                return true;
            }

            return false;
        }
        #endregion

        public virtual void Dispose() {}

        public static implicit operator Tween(Action<float> value)
        {
            return new Tween(value);
        }
    }
}
