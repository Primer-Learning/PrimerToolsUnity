using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class ComponentExtensions
    {
        public static async UniTask ReplaceWith(this Component left, Component other)
        {
            var snapshotLeft = new TransformSnapshot(left.transform);
            var snapshotRight = new TransformSnapshot(other.transform);

            await left.GetPrimer().ScaleDownToZero();
            snapshotLeft.ApplyTo(other.transform);
            await other.GetPrimer().ScaleUpFromZero();
        }
    }
}
