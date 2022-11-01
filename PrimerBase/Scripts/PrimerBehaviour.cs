using System.Threading.Tasks;
using UnityEngine;
using Primer;

[AddComponentMenu("Primer Learning / PrimerBehaviour")]
public class PrimerBehaviour : MonoBehaviour
{
    PrimerAnimation _tween;
    protected PrimerAnimation tween => _tween ?? (_tween = gameObject.AddComponent<PrimerAnimation>());

    public async Task MoveTo(Vector3 target, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) =>
        await tween.MoveTo(target, duration, ease);

    public async void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) =>
        await ScaleUpFromZeroAwaitable(duration, ease);

    public async Task ScaleUpFromZeroAwaitable(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) =>
        await tween.ScaleUpFromZero(duration, ease);

    public async void ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) =>
        await tween.ScaleUpFromZero(duration, ease);

    public async void ShrinkAndDispose(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            await tween.ScaleDownToZero(duration, ease);
        }

        // This is false if the element has already been destroyed
        if (this) {
            gameObject.Dispose();
        }
    }
}
