using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class TransformExtensions
    {
        [Obsolete("Use transform.GetPrimer().ScaleUpFromZero() instead")]
        public static async UniTask ScaleUpFromZero(this Transform transform, Tweener animation = null)
        {
            await transform.GetPrimer().ScaleUpFromZero(animation);
        }

        [Obsolete("Use transform.GetPrimer().ScaleDownToZero() instead")]
        public static async UniTask ScaleDownToZero(this Transform transform, Tweener animation = null)
        {
            await transform.GetPrimer().ScaleDownToZero(animation);
        }

        // Convenience overload method for scaling to multiples of Vector3.one
        public static async UniTask ScaleTo(this Transform transform, float newScale,
            Tweener animation = null, CancellationToken ct = default)
        {
            await transform.ScaleTo(newScale * Vector3.one, animation: animation, ct: ct);
        }

        public static async UniTask ScaleTo(this Transform transform, Vector3 newScale,
            Tweener animation = null, CancellationToken ct = default)
        {
            if (transform == null || transform.localScale == newScale) return;

            if (!Application.isPlaying) {
                transform.localScale = newScale;
                return;
            }

            await foreach (var scale in animation.Tween(transform.localScale, newScale, ct)) {
                if (ct.IsCancellationRequested) return;
                transform.localScale = scale;
            }
        }

        public static async UniTask MoveTo(this Transform transform, Vector3 newPosition,
            Tweener animation = null, CancellationToken ct = default)
        {
            if (transform == null || transform.localPosition == newPosition) return;

            if (!Application.isPlaying) {
                transform.localPosition = newPosition;
                return;
            }

            await foreach (var pos in animation.Tween(transform.localPosition, newPosition, ct)) {
                if (ct.IsCancellationRequested) return;
                transform.localPosition = pos;
            }
        }
    }
}
