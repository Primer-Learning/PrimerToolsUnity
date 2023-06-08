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
    }
}
