using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Extensions
{
    public static class TransformExtensions
    {
        public static Dictionary<Transform, Vector3> originalScales = new();

        public async static UniTask ScaleUpFromZero(
            this Transform transform,
            float duration = PrimerAnimation.DEFAULT_DURATION,
            EaseMode ease = PrimerAnimation.DEFAULT_EASING)
        {
            if (transform == null) return;

            var originalScale = transform.localScale;

            if (!originalScales.TryAdd(transform, originalScale))
                originalScale = originalScales[transform];

            transform.localScale = Vector3.zero;
            await scaleTo(transform, originalScale, duration, ease);

            originalScales.Remove(transform);
        }

        public async static UniTask ScaleDownToZero(
            this Transform transform,
            float duration = PrimerAnimation.DEFAULT_DURATION,
            EaseMode ease = PrimerAnimation.DEFAULT_EASING)
        {
            if (transform == null || transform.localScale == Vector3.zero) return;

            var originalScale = transform.localScale;

            if (!originalScales.TryAdd(transform, originalScale))
                originalScales[transform] = originalScale;

            await scaleTo(transform, Vector3.zero, duration, ease);
        }

        private async static UniTask scaleTo(Transform transform, Vector3 newScale, float duration, EaseMode ease)
        {
            var ct = new CancellationToken();

            await foreach (var scale in PrimerAnimation.Tween(ct, transform.localScale, newScale, duration, ease)) {
                transform.localScale = scale;
            }
        }
    }
}
