using System;
using System.Threading.Tasks;
using UnityEngine;
[AddComponentMenu("Primer Learning / PrimerBehaviour")]
public class PrimerBehaviour : MonoBehaviour
{
    protected Vector3 intrinsicScale = new Vector3(1, 1, 1);

    public void SetIntrinsicScale(Vector3 scale) {
        intrinsicScale = scale;
    }
    public void SetIntrinsicScale(float scale) {
        intrinsicScale = new Vector3(scale, scale, scale);
    }
    public void SetIntrinsicScale() {
        intrinsicScale = transform.localScale;
    }

    public async void ShrinkAndDispose() {
        if (!Application.isEditor) {
            await ScaleDownToZero();
        }

        gameObject.Dispose();
    }

    public async Task ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float delay = 0) {
        if (Application.isEditor) return;
        transform.localScale = Vector3.zero;
        await scaleTo(intrinsicScale, duration, ease, delay);
    }

    public async Task ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isEditor) {
            transform.localScale = Vector3.zero;
        }
        else {
            await scaleTo(Vector3.zero, duration, ease);
        }
    }

    async Task scaleTo(Vector3 newScale, float duration, EaseMode ease, float delay = 0) {
        if (delay > 0) {
            await Task.Delay((int)(delay * 1000));
        }

        await tween(
            transform.localScale,
            newScale,
            duration,
            ease,
            x => transform.localScale = x
        );
    }

    async Task tween<T>(T initial, T target, float duration, EaseMode ease, Action<T> execute) {
        var startTime = Time.time;
        var Lerp = typeof(T).GetMethod("Lerp");

        if (Lerp == null) {
            throw new ArgumentException($"PrimerBehaviour.tween() couldn't find .Lerp() in {typeof(T).FullName}");
        }

        while (Time.time < startTime + duration) {
            var t = (Time.time - startTime) / duration;
            var eased = Helpers.ApplyNormalizedEasing(t, ease);
            var lerp = Lerp.Invoke(null, new object[] {
                initial, target, eased
            });

            execute((T)lerp);
            await Task.Yield();
        }

        execute(target);
    }
}
