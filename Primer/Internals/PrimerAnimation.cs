using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Primer
{
    public class PrimerAnimation : MonoBehaviour
    {
        /// <summary>
        ///     Frequency in milliseconds for an animation execution
        /// </summary>
        private const int TWEEN_DELAY = 1000 / 60;

        public const float DEFAULT_DURATION = 0.5f;
        public const EaseMode DEFAULT_EASING = EaseMode.SmoothStep;

        public async static IAsyncEnumerable<T> Tween<T>(
            [EnumeratorCancellation]
            CancellationToken ct,
            T initial,
            T target,
            float duration = DEFAULT_DURATION,
            EaseMode ease = DEFAULT_EASING)
        {
            var startTime = Time.time;
            var lerp = typeof(T).GetMethod("Lerp");

            if (lerp == null) {
                throw new ArgumentException($"PrimerAnimation.tween() couldn't find .Lerp() in {typeof(T).FullName}");
            }

            while (!ct.IsCancellationRequested && Time.time < startTime + duration) {
                var t = (Time.time - startTime) / duration;
                var tEased = Easing.ApplyNormalizedEasing(t, ease);
                var lerped = lerp.Invoke(null, new object[] {
                    initial, target, tEased
                });

                yield return (T)lerped;
                await Task.Delay(TWEEN_DELAY, ct);
            }

            yield return target;
        }

        // Instance fields

        private Vector3? originalScale;
        private readonly CancellationTokenSource cancelAnimations = new();

        private void OnDestroy()
        {
            cancelAnimations.Cancel();
        }

        public async Task MoveTo(Vector3 target, float duration, EaseMode ease)
        {
            if (!this) return;

            if (Application.isPlaying)
                await moveTo(cancelAnimations.Token, target, duration, ease);
            else
                transform.localPosition = target;

        }

        private async Task moveTo(CancellationToken ct, Vector3 target, float duration, EaseMode ease)
        {
            // If the component is already destroyed do nothing
            if (!this) return;

            await foreach (var pos in Tween(ct, transform.localPosition, target, duration, ease)) {
                if (!ct.IsCancellationRequested) {
                    transform.localPosition = pos;
                }
            }
        }

        public async Task ScaleUpFromZero(float duration, EaseMode ease)
        {
            if (!this) return;

            if (!Application.isPlaying) return;
            SaveOriginalScale();
            transform.localScale = Vector3.zero;
            await scaleTo(cancelAnimations.Token, (Vector3)originalScale, duration, ease);
        }

        public async Task ScaleDownToZero(float duration, EaseMode ease)
        {
            if (!this) return;

            if (Application.isPlaying)
                await scaleTo(cancelAnimations.Token, Vector3.zero, duration, ease);
            else
                transform.localScale = Vector3.zero;
        }

        private async Task scaleTo(CancellationToken ct, Vector3 newScale, float duration, EaseMode ease)
        {
            if (!this) return;

            await foreach (var scale in Tween(ct, transform.localScale, newScale, duration, ease)) {
                if (!ct.IsCancellationRequested) {
                    transform.localScale = scale;
                }
            }
        }

        private void SaveOriginalScale()
        {
            if (originalScale is null) {
                originalScale = transform.localScale;
            }
        }
    }
}
