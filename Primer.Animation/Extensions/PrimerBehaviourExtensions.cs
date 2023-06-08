using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class PrimerBehaviourExtensions
    {
        public static Tween ScaleDownToZero(this PrimerBehaviour self)
        {
            self.FindIntrinsicScale();
            return self.transform.ScaleTo(Vector3.zero);
        }

        public static Tween ScaleUpFromZero(this PrimerBehaviour self)
        {
            var target = self.FindIntrinsicScale();
            self.transform.localScale = Vector3.zero;
            self.SetActive(true);
            return self.transform.ScaleTo(target);
        }

        public static Tween MoveTo(this PrimerBehaviour self, Vector3 target)
        {
            self.FindIntrinsicPosition();
            return self.transform.MoveTo(target);
        }

        public static async UniTask ShrinkAndDispose(this PrimerBehaviour self, float duration = 0.5f, CancellationToken ct = default)
        {
            if (!self)
                return;

            if (Application.isPlaying) {
                var tween = self.ScaleDownToZero() with { duration = duration };
                await tween.Play(ct);
            }

            // This is false if the element has already been destroyed
            if (self)
                self.gameObject.Dispose();
        }
    }
}
