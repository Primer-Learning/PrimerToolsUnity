using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public partial record Tween(Action<float> lerp) : IDisposable
    {
        public static Tween noop = new(_ => {});

        public IEasing easeMethod { get; init; } = IEasing.defaultMethod;
        public EaseMode ease {
            get => EaseModeExtensions.GetModeFor(easeMethod);
            init => easeMethod = value.GetMethod();
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
            if (delay is not 0)
                t = Mathf.Clamp01(PrimerMath.Remap(tStart, 1, 0, 1, t));

            lerp(easeMethod.Evaluate(t));
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

            Evaluate(0);

            while (!ct.IsCancellationRequested && Time.time < startTime + delayAndDuration) {
                var t = (Time.time - startTime) / delayAndDuration;

                Evaluate(t);
                await UniTask.DelayFrame(1, cancellationToken: ct);

                if (ct.IsCancellationRequested)
                    return;
            }

            Evaluate(1);
        }
        #endregion

        public virtual void Dispose() {}

        public static implicit operator Tween(Action<float> value)
        {
            return new Tween(value);
        }
    }
}
