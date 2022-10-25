using System;
using System.Collections.Generic;
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

        public async static IAsyncEnumerable<T> Tween<T>(T initial, T target, float duration, EaseMode ease) {
            var startTime = Time.time;
            var Lerp = typeof(T).GetMethod("Lerp");

            if (Lerp == null) {
                throw new ArgumentException($"PrimerAnimation.tween() couldn't find .Lerp() in {typeof(T).FullName}");
            }

            while (Time.time < startTime + duration) {
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

        public async Task ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
            if (!Application.isPlaying) return;
            SaveOriginalScale();
            transform.localScale = Vector3.zero;
            await scaleTo((Vector3)originalScale, duration, ease);
        }

        public async Task ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
            if (Application.isPlaying) {
                await scaleTo(Vector3.zero, duration, ease);
            }
            else {
                transform.localScale = Vector3.zero;
            }
        }

        async Task scaleTo(Vector3 newScale, float duration, EaseMode ease) {
            await foreach (var scale in Tween(transform.localScale, newScale, duration, ease)) {
                transform.localScale = scale;
            }
        }

        void SaveOriginalScale() {
            if (originalScale is null) {
                originalScale = transform.localScale;
            }
        }
    }
}
