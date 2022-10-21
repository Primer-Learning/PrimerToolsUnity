using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
[AddComponentMenu("Primer Learning / PrimerBehaviour")]
public class PrimerBehaviour : MonoBehaviour
{
    const int TWEEN_DELAY = 10;

    public async void ShrinkAndDispose() {
        if (Application.isPlaying) {
            await ScaleDownToZero();
        }

        gameObject.Dispose();
    }

    public async void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float delay = 0) {
        if (!Application.isPlaying) return;
        var start = transform.localScale;
        transform.localScale = Vector3.zero;
        await scaleTo(start, duration, ease, delay);
    }

    public async Task ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            await scaleTo(Vector3.zero, duration, ease);
        }
        else {
            transform.localScale = Vector3.zero;
        }
    }

    async Task scaleTo(Vector3 newScale, float duration, EaseMode ease, float delay = 0) {
        if (delay > 0) {
            await Task.Delay((int)(delay * 1000));
        }

        await foreach (var scale in tween(transform.localScale, newScale, duration, ease)) {
            transform.localScale = scale;
        }
    }



    async protected IAsyncEnumerable<T> tween<T>(T initial, T target, float duration, EaseMode ease) {
        var startTime = Time.time;
        var Lerp = typeof(T).GetMethod("Lerp");

        if (Lerp == null) {
            throw new ArgumentException($"PrimerBehaviour.tween() couldn't find .Lerp() in {typeof(T).FullName}");
        }

        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            var tEased = Helpers.ApplyNormalizedEasing(t, ease);
            var lerp = Lerp.Invoke(null, new object[] {
                initial, target, tEased
            });

            yield return (T)lerp;
            await Task.Delay(TWEEN_DELAY);
        }

        yield return target;
    }
}
