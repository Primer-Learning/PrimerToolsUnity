using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class IPrimer_ShrinkAndDisposeExtensions
    {
        public static async UniTask ShrinkAndDispose(this IPrimer self, float duration = 0.5f,
            CancellationToken ct = default)
        {
            if (self == null)
                return;

            if (Application.isPlaying) {
                var tween = self.ScaleDownToZero() with { duration = duration };
                await tween.Play(ct);
            }

            // This is false if the element has already been destroyed while we were scaling down
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (self != null)
                self.component.gameObject.Dispose();
        }

        public static UniTask ShrinkAndDispose(this Component self, float duration = 0.5f,
            CancellationToken ct = default)
        {
            return self.ToPrimer().ShrinkAndDispose(duration, ct);
        }
    }
}
