using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Animation
{
    public static class TransformExtensions
    {
        // [Obsolete("Use transform.GetPrimer().ScaleUpFromZero() instead")]
        // public static Tween ScaleUpFromZero(this Transform transform)
        // {
        //     return transform.GetPrimer().ScaleUpFromZero();
        // }
        //
        // [Obsolete("Use transform.GetPrimer().ScaleDownToZero() instead")]
        // public static Tween ScaleDownToZero(this Transform transform)
        // {
        //     return transform.GetPrimer().ScaleDownToZero();
        // }

        // Convenience overload method for scaling to multiples of Vector3.one
        public static Tween ScaleTo(this Transform transform, float newScale)
        {
            return transform.ScaleTo(newScale * Vector3.one);
        }

        public static Tween ScaleTo(this Transform transform, Vector3 newScale)
        {
            var initial = transform.localScale;

            return initial == newScale
                ? Tween.noop
                : new Tween(t => transform.localScale = Vector3.Lerp(initial, newScale, t));
        }

        public static Tween MoveTo(this Transform transform, Vector3 newPosition)
        {
            var initial = transform.localPosition;

            return initial == newPosition
                ? Tween.noop
                : new Tween(t => transform.localPosition = Vector3.Lerp(initial, newPosition, t));
        }
    }
}
