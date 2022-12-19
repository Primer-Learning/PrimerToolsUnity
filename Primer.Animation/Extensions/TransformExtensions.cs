using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class TransformExtensions
    {
        public async static UniTask ScaleUpFromZero(this Transform transform, Tweener animation = null)
        {
            await transform.GetOrAddComponent<PrimerBehaviour>().ScaleUpFromZero(animation);
        }

        public async static UniTask ScaleDownToZero(this Transform transform, Tweener animation = null)
        {
            await transform.GetOrAddComponent<PrimerBehaviour>().ScaleDownToZero(animation);
        }

        public async static UniTask ScaleTo(this Transform transform, Vector3 newScale,
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

        public async static UniTask MoveTo(this Transform transform, Vector3 newPosition,
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
