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
        
        public static Tween ScaleTo(this PrimerBehaviour self, Vector3 newScale)
        {
            self.FindIntrinsicScale();
            return self.transform.ScaleTo(newScale);
        }
        
        public static Tween ScaleUpFromZero(this PrimerBehaviour self)
        {
            var target = self.FindIntrinsicScale();
            self.transform.localScale = Vector3.zero;
            return self.transform.ScaleTo(target);
        }

        public static Tween MoveTo(this PrimerBehaviour self, Vector3 target)
        {
            self.FindIntrinsicPosition();
            return self.transform.MoveTo(target);
        }

        // public static async UniTask ShrinkAndDispose(this PrimerBehaviour self, Tweener anim = null)
        // {
        //     if (!self)
        //         return;
        //
        //     if (Application.isPlaying)
        //         await self.ScaleDownToZero(anim);
        //
        //     // This is false if the element has already been destroyed
        //     if (self)
        //         self.gameObject.Dispose();
        // }
        //
        //
        // #region Ensure that only one animation per PrimerBehaviour is running at a time
        // private static readonly Dictionary<PrimerBehaviour, CancellationTokenSource> isScaling = new();
        //
        // private static CancellationToken CreateCancellationToken(PrimerBehaviour self)
        // {
        //     if (isScaling.TryGetValue(self, out var token))
        //     {
        //         token.Cancel();
        //         token.Dispose();
        //     }
        //
        //     token = new CancellationTokenSource();
        //     self.CancelOnDestroy(token);
        //     isScaling[self] = token;
        //     return token.Token;
        // }
        //
        // private static void ClearToken(PrimerBehaviour self)
        // {
        //     if (!isScaling.TryGetValue(self, out var token))
        //         return;
        //
        //     token.Cancel();
        //     token.Dispose();
        //     isScaling.Remove(self);
        // }
        // #endregion
    }
}
