using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class PrimerBehaviourExtensions
    {
        public static async UniTask ScaleDownToZero(this PrimerBehaviour self, Tweener anim = null)
        {
            if (self.transform.localScale == Vector3.zero)
                return;

            self.FindIntrinsicScale();
            var ct = CreateCancellationToken(self);
            await self.transform.ScaleTo(Vector3.zero, anim, ct);
            ClearToken(self);
        }

        public static async UniTask ScaleUpFromZero(this PrimerBehaviour self, Tweener anim = null)
        {
            var target = self.FindIntrinsicScale();

            if (self.transform.localScale == target)
                return;

            if (!Application.isPlaying) {
                self.transform.localScale = target;
                return;
            }

            self.transform.localScale = Vector3.zero;
            var ct = CreateCancellationToken(self);
            await self.transform.ScaleTo(target, anim, ct);
            ClearToken(self);
        }

        public static async UniTask MoveTo(this PrimerBehaviour self, Vector3 target, Tweener anim = null)
        {
            if (self.transform.localPosition == target)
                return;

            if (!Application.isPlaying) {
                self.transform.localPosition = target;
                return;
            }

            self.FindIntrinsicPosition();
            var ct = CreateCancellationToken(self);
            await self.transform.MoveTo(target, anim, ct);
            ClearToken(self);
        }

        public static async void ShrinkAndDispose(this PrimerBehaviour self, Tweener anim = null)
        {
            if (!self)
                return;

            if (Application.isPlaying)
                await self.ScaleDownToZero(anim);

            // This is false if the element has already been destroyed
            if (self)
                self.gameObject.Dispose();
        }


        #region Ensure that only one animation per PrimerBehaviour is running at a time
        private static readonly Dictionary<PrimerBehaviour, CancellationTokenSource> isScaling = new();

        private static CancellationToken CreateCancellationToken(PrimerBehaviour self)
        {
            if (isScaling.TryGetValue(self, out var token))
            {
                token.Cancel();
                token.Dispose();
            }

            token = new CancellationTokenSource();
            self.CancelOnDestroy(token);
            isScaling[self] = token;
            return token.Token;
        }

        private static void ClearToken(PrimerBehaviour self)
        {
            if (!isScaling.TryGetValue(self, out var token))
                return;

            token.Cancel();
            token.Dispose();
            isScaling.Remove(self);
        }
        #endregion
    }
}
