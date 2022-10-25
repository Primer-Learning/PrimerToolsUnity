using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using Primer;

[AddComponentMenu("Primer Learning / PrimerBehaviour")]
public class PrimerBehaviour : MonoBehaviour
{
    PrimerAnimation _tween;
    protected PrimerAnimation tween => _tween ?? (_tween = gameObject.AddComponent<PrimerAnimation>());

    public async void ScaleUpFromZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) =>
        await tween.ScaleUpFromZero(duration, ease);

    public async void ScaleDownToZero(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) =>
        await tween.ScaleUpFromZero(duration, ease);

    public async void ShrinkAndDispose() {
        if (Application.isPlaying) {
            await tween.ScaleDownToZero();
        }

        gameObject.Dispose();
    }
}
