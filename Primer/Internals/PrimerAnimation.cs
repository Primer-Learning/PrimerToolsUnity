using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer
{
    public record PrimerAnimation(float duration = 0.5f, EaseMode ease = EaseMode.Cubic);


    public static class PrimerAnimationExtensions {
        private static readonly PrimerAnimation @default = new();

        /// <summary>
        ///     This method is implemented as an extension so it can be called even on `null` objects
        /// </summary>
        public async static IAsyncEnumerable<T> Tween<T>(this PrimerAnimation config, T initial, T target,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var startTime = Time.time;
            var animation = config ?? @default;
            var lerp = typeof(T).GetMethod("Lerp");

            if (lerp == null) {
                throw new ArgumentException(
                    $"AnimationConfig.tween() couldn't find static .Lerp() in {typeof(T).FullName}");
            }

            while (!ct.IsCancellationRequested && Time.time < startTime + animation.duration) {
                var t = (Time.time - startTime) / animation.duration;
                var tEased = Easing.ApplyNormalizedEasing(t, animation.ease);
                var lerped = lerp.Invoke(null, new object[] {
                    initial, target, tEased
                });

                yield return (T)lerped;
                await UniTask.DelayFrame(1, cancellationToken: ct);
            }

            if (!ct.IsCancellationRequested)
                yield return target;
        }
    }
}
