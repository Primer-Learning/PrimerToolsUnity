using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Primer.Animation
{
    public record Tween(Action<float> lerp)
    {
        public static Tween noop = new Tween(_ => { });

        public IEasing easeMethod = IEasing.defaultMethod;
        public EaseMode ease {
            get => EaseModeExtensions.GetModeFor(easeMethod);
            set => easeMethod = value.GetMethod();
        }

        private bool isCalculated = false;
        private int _milliseconds = 500;

        public int milliseconds {
            get => _milliseconds;
            set  {
                if (isCalculated) {
                    Debug.LogWarning("Forcing the duration of a calculated tween");
                    isCalculated = false;
                }

                _milliseconds = value;
            }
        }

        public float seconds {
            get => milliseconds / 1000f;
            set => milliseconds = (int) (value * 1000);
        }

        public float duration {
            get => milliseconds / 1000f;
            set => milliseconds = (int) (value * 1000);
        }

        public float totalDuration => duration + delay;

        public float delay = 0f;

        public void Evaluate(float t)
        {
            if (delay is not 0)
                t = Mathf.Clamp01(PrimerMath.Remap(0, 1, -delay / duration, 1, t));

            lerp(easeMethod.Evaluate(t));
        }

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

                if (!ct.IsCancellationRequested)
                    return;
            }

            Evaluate(1);
        }

        public static implicit operator Tween(Action<float> value)
        {
            return new Tween(value);
        }

        public static Tween Merge(params Tween[] tweenList)
        {
            return new Tween(
                t => {
                    for (var i = 0; i < tweenList.Length; i++) {
                        tweenList[i].Evaluate(t);
                    }
                }
            ) {
                isCalculated = true,
                duration = tweenList.Max(x => x.totalDuration),
            };
        }
    }
}
