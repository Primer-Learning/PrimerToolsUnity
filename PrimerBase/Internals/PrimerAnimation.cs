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
        /// Frequency in milliseconds for an animation execution
        /// </summary>
        const int TWEEN_DELAY = 1000 / 60;

        public async static IAsyncEnumerable<T> Tween<T>([EnumeratorCancellation] CancellationToken ct, T initial, T target, float duration, EaseMode ease) {
            var startTime = Time.time;
            var Lerp = typeof(T).GetMethod("Lerp");

            if (Lerp == null) {
                throw new ArgumentException($"PrimerAnimation.tween() couldn't find .Lerp() in {typeof(T).FullName}");
            }

            while (!ct.IsCancellationRequested && Time.time < startTime + duration) {
                var t = (Time.time - startTime) / duration;
                var tEased = Easing.ApplyNormalizedEasing(t, ease);
                var lerp = Lerp.Invoke(null, new object[] {
                    initial, target, tEased
                });

                yield return (T)lerp;
                await Task.Delay(TWEEN_DELAY);
            }

            yield return target;
        }

        // Instance fields

        Vector3? originalScale = null;
        CancellationTokenSource cancelAnimations = new CancellationTokenSource();

        void OnDestroy() {
            cancelAnimations.Cancel();
        }

        public async Task MoveTo(Vector3 target, float duration, EaseMode ease) {
            if (!this) return;

            if (Application.isPlaying)
                await moveTo(cancelAnimations.Token, target, duration, ease);
            else
                transform.localPosition = target;

        }

        async Task moveTo(CancellationToken ct, Vector3 target, float duration, EaseMode ease) {
            // If the component is already destroyed do nothing
            if (!this) return;

            await foreach (var pos in Tween(ct, transform.localPosition, target, duration, ease)) {
                if (!ct.IsCancellationRequested) {
                    transform.localPosition = pos;
                }
            }
        }

        public async Task ScaleUpFromZero(float duration, EaseMode ease) {
            if (!this) return;

            if (!Application.isPlaying) return;
            SaveOriginalScale();
            transform.localScale = Vector3.zero;
            await scaleTo(cancelAnimations.Token, (Vector3)originalScale, duration, ease);
        }

        public async Task ScaleDownToZero(float duration, EaseMode ease) {
            if (!this) return;

            if (Application.isPlaying)
                await scaleTo(cancelAnimations.Token, Vector3.zero, duration, ease);
            else
                transform.localScale = Vector3.zero;
        }

        async Task scaleTo(CancellationToken ct, Vector3 newScale, float duration, EaseMode ease) {
            if (!this) return;

            await foreach (var scale in Tween(ct, transform.localScale, newScale, duration, ease)) {
                if (!ct.IsCancellationRequested) {
                    transform.localScale = scale;
                }
            }
        }

        void SaveOriginalScale() {
            if (originalScale is null) {
                originalScale = transform.localScale;
            }
        }
    }
}
