using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public record Tween(Action<float> lerp) : IDisposable
    {
        public static Tween noop = new Tween(_ => { });

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
            init  {
                if (isCalculated) {
                    Debug.LogWarning("Forcing the duration of a calculated tween");
                    isCalculated = false;
                }

                ms = value;
            }
        }

        public float seconds {
            get => milliseconds / 1000f;
            init => milliseconds = (int) (value * 1000);
        }

        public float duration {
            get => milliseconds / 1000f;
            init => milliseconds = (int) (value * 1000);
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

        public virtual void Dispose() {}


        #region Convinience methods
        public Tween Delay(float amount)
        {
            return this with { delay = amount };
        }
        #endregion


        #region Operators
        public static implicit operator Tween(Action<float> value)
        {
            return new Tween(value);
        }
        #endregion


        #region Static methods: Parallel & Series
        public static Tween Parallel(params Tween[] tweenList)
        {
            if (tweenList.Length == 0)
                return noop with { milliseconds = 0 };
            
            var fullDuration = tweenList.Max(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Parallel tween list is empty");
                return noop with { milliseconds = 0 };
            }

            return new Tween(
                t => {
                    for (var i = 0; i < tweenList.Length; i++) {
                        var tween = tweenList[i];
                        tween.Evaluate(Mathf.Clamp01(t / tween.totalDuration * fullDuration));
                    }
                }
            ) {
                easeMethod = LinearEasing.instance,
                duration = fullDuration,
                isCalculated = true,
            };
        }

        public static Tween Parallel(float delayBetweenStarts, params Tween[] tweenList)
        {
            return Parallel(tweenList.Select((tween, i) => tween with { delay = delayBetweenStarts * i }).ToArray());
        }
        
        public static Tween Series(params Tween[] tweenList)
        {
            var fullDuration = tweenList.Sum(x => x.totalDuration);

            if (fullDuration is 0) {
                Debug.LogWarning("Series tween list is empty");
                return noop with { milliseconds = 0 };
            }

            var cursor = 0;
            var cursorStart = 0f;
            var cursorEnd = tweenList[0].duration;
            var cursorEndT = cursorEnd / fullDuration;

            return new Tween(
                t => {
                    while (t > cursorEndT) {
                        cursor++;
                        cursorStart = cursorEnd;
                        cursorEnd += tweenList[cursor].duration;
                        cursorEndT = cursorEnd / fullDuration;
                    }

                    // formula wrote by copilot, is it right?
                    t = Mathf.Clamp01((t - cursorStart) / (cursorEnd - cursorStart));
                    tweenList[cursor].Evaluate(t);
                }
            ) {
                easeMethod = LinearEasing.instance,
                duration = fullDuration,
                isCalculated = true,
            };
        }
        #endregion
    }
}
