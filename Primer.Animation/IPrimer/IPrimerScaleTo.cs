
using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface to add custom scaling behavior to your Primer.
    /// This can be used to do actions before/after scaling
    /// </summary>
    public interface IPrimerScaleTo : ITransformHolder {
        Tween ScaleTo(Vector3 newScale, Vector3 initialScale);
    }


    public static class PrimerScaleToExtensions {
        #region Overloads on the interface
        // These methods just adds different ways newPosition call a ICustomPrimerRotation

        public static Tween ScaleTo(this IPrimerScaleTo self, float newScale, float? initialScale = null)
        {
            var from = initialScale.HasValue
                ? Vector3.one * initialScale.Value
                : self.transform.localScale;

            return self.ScaleTo(Vector3.one * newScale, from);
        }

        public static Tween ScaleTo(this IPrimerScaleTo self, Vector3 newScale, Vector3? initialScale = null)
        {
            return self.ScaleTo(newScale, initialScale ?? self.transform.localScale);
        }

        public static Tween ScaleUpFromZero(this IPrimerScaleTo self, Vector3? targetScale = null)
        {
            return self.ScaleTo(targetScale ?? self.transform.localScale, initialScale: Vector3.zero);
        }

        public static Tween ScaleDownToZero(this IPrimerScaleTo self, Vector3? initialScale = null)
        {
            return self.ScaleTo(Vector3.zero, initialScale ?? self.transform.localScale);
        }
        #endregion


        #region Default implementation for Component: redirect to Transform
        public static Tween ScaleTo(
            this Component self,
            float newScale,
            float? initialScale = null)
        {
            var from = initialScale.HasValue
                ? Vector3.one * initialScale.Value
                : self.transform.localScale;

            return self.transform.ScaleTo(Vector3.one * newScale, from);
        }

        public static Tween ScaleTo(
            this Component self,
            Vector3 newScale,
            Vector3? initialScale = null)
        {
            return self.transform.ScaleTo(newScale, initialScale);
        }

        public static Tween ScaleUpFromZero(this Component self, Vector3? targetScale = null)
        {
            return self.transform.ScaleTo(targetScale ?? self.transform.localScale, initialScale: Vector3.zero);
        }

        public static Tween ScaleDownToZero(this Component self, Vector3? initialScale = null)
        {
            return self.transform.ScaleTo(Vector3.zero, initialScale ?? self.transform.localScale);
        }
        #endregion


        #region Actual implementation in Transform
        public static Tween ScaleTo(
            this Transform self,
            float newScale,
            float? initialScale = null)
        {
            var from = initialScale.HasValue
                ? Vector3.one * initialScale.Value
                : self.transform.localScale;

            return self.ScaleTo(Vector3.one * newScale, from);
        }

        public static Tween ScaleTo(
            this Transform self,
            Vector3 newScale,
            Vector3? initialScale = null)
        {
            var from = initialScale.HasValue
                ? initialScale.Value
                : self.transform.localScale;

            return initialScale == newScale
                ? Tween.noop
                : new Tween(t => self.localScale = Vector3.Lerp(from, newScale, t));
        }

        public static Tween ScaleUpFromZero(this Transform self, Vector3? targetScale = null)
        {
            return self.ScaleTo(targetScale ?? self.localScale, initialScale: Vector3.zero);
        }

        public static Tween ScaleDownToZero(this Transform self, Vector3? initialScale = null)
        {
            return self.ScaleTo(Vector3.zero, initialScale ?? self.transform.localScale);
        }
        #endregion
    }
}
