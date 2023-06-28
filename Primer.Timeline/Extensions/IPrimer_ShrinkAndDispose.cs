using System.Threading;
using Cysharp.Threading.Tasks;
using Primer.Timeline;
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

            if (PrimerTimeline.isPlaying) {
                var tween = self.ScaleDownToZero() with { duration = duration };
                await tween.Play(ct);
            }

            // This is false if the element has already been destroyed while we were scaling down
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (self.transform != null)
                self.transform.gameObject.Dispose();
        }

        public static UniTask ShrinkAndDispose(this Component self, float duration = 0.5f,
            CancellationToken ct = default)
        {
            return self.ToPrimer().ShrinkAndDispose(duration, ct);
        }
    }
}
