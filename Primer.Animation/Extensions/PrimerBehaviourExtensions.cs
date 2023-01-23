using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class PrimerBehaviourExtensions
    {
        public static async UniTask ScaleDownToZero(this PrimerBehaviour self, Tweener anim = null)
        {
            self.FindIntrinsicScale();
            await self.transform.ScaleTo(Vector3.zero, anim, self.lifetime);
        }

        public static async UniTask ScaleUpFromZero(this PrimerBehaviour self, Tweener anim = null)
        {
            var target = self.FindIntrinsicScale();
            self.transform.localScale = Vector3.zero;
            await self.transform.ScaleTo(target, anim, self.lifetime);
        }

        public static async UniTask MoveTo(this PrimerBehaviour self, Vector3 target, Tweener anim = null)
        {
            self.FindIntrinsicPosition();
            await self.transform.MoveTo(target, anim, self.lifetime);
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
    }
}
