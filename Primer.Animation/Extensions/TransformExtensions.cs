using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Primer.Animation
{
    public static class TransformExtensions
    {
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
        public static Tween MoveBy(this Transform transform, Vector3 displacement)
        {
            var initial = transform.localPosition;
            var newPosition = initial + displacement; 

            return initial == newPosition
                ? Tween.noop
                : new Tween(t => transform.localPosition = Vector3.Lerp(initial, newPosition, t));
        }
        
        public static Tween RotateTo(this Transform transform, Quaternion newRotation)
        {
            var initial = transform.localRotation;

            try
            {
                return new Tween(t => transform.localRotation = Quaternion.Lerp(initial, newRotation, t));
            }
            catch
            {
                Debug.LogWarning("Tween failed in RotateTo. Quaternions may be too close.");
                return Tween.noop;
            }
            
            // The code below did not work.
            // GPT 4 says quaternion equality check is unreliable, and that Unity does not allow lerping of quaternions
            // that are very close together, for some reason.
            return initial == newRotation
                ? Tween.noop
                : new Tween(t => transform.localRotation = Quaternion.Lerp(initial, newRotation, t));
        }
    }
}