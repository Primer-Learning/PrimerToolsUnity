using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
[AddComponentMenu("Primer Learning / PrimerBehaviour")]
public class PrimerBehaviour : MonoBehaviour
{
    public async void ShrinkAndDispose() {
        if (!Application.isEditor) {
            await ScaleDownToZero();
        }

        gameObject.Dispose();
    }

    public T GenerateChild<T>(T template, Transform parent) where T : Object {
        var child = Instantiate(template, parent);
        child.name = $"{GameObjectExtensions.GENERATED_GAME_OBJECT_PREFIX}{template.name}";
        return child;
    }

    public async Task ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic, float delay = 0) {
        if (Application.isEditor) return;
        var start = transform.localScale;
        transform.localScale = Vector3.zero;
        await scaleTo(start, duration, ease, delay);
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

        foreach (var scale in tween(transform.localScale, newScale, duration, ease)) {
            transform.localScale = scale;
        }
    }

    IEnumerable<T> tween<T>(T initial, T target, float duration, EaseMode ease) {
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
        }

        yield return target;
    }
}
