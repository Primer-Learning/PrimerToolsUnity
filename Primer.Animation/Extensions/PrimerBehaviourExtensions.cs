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

        public static async UniTask ShrinkAndDispose(this PrimerBehaviour self, CancellationToken ct = default)
        {
            if (!self)
                return;

            if (Application.isPlaying)
                await self.ScaleDownToZero().Play(ct);

            // This is false if the element has already been destroyed
            if (self)
                self.gameObject.Dispose();
        }
    }
}
