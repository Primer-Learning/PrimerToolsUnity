using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface to add custom scaling behavior to your IPrimer or Component.
    /// This can be used to do actions before/after scaling by returning transform.ScaleTo(...).Observe(...)
    /// </summary>
    public interface IPrimer_CustomScaleTo
    {
        Tween ScaleTo(Vector3 newScale, Vector3 initialScale);
    }


    public static class IPrimer_ScaleToExtensions
    {
        #region Overloads for IEnumerable<IPrimer>
        public static IEnumerable<Tween> ScaleUpFromZero(this IEnumerable<IPrimer> self, Vector3? targetScale = null)
            => self.Select(x => x.ScaleUpFromZero(targetScale));

        public static IEnumerable<Tween> ScaleDownToZero(this IEnumerable<IPrimer> self, Vector3? initialScale = null)
            => self.Select(x => x.ScaleDownToZero(initialScale));

        public static IEnumerable<Tween> ScaleTo(this IEnumerable<IPrimer> self, float newScale,
            float? initialScale = null) => self.Select(x => x.ScaleTo(newScale, initialScale));

        public static IEnumerable<Tween> ScaleTo(this IEnumerable<IPrimer> self, Vector3 newScale,
            Vector3? initialScale = null) => self.Select(x => x.ScaleTo(newScale, initialScale));

        public static IEnumerable<Tween> ScaleUpFromZero(this IEnumerable<Transform> self, Vector3? targetScale = null)
            => self.Select(x => x.ScaleUpFromZero(targetScale));

        public static IEnumerable<Tween> ScaleDownToZero(this IEnumerable<Transform> self, Vector3? initialScale = null)
            => self.Select(x => x.ScaleDownToZero(initialScale));

        public static IEnumerable<Tween> ScaleTo(this IEnumerable<Transform> self, float newScale,
            float? initialScale = null) => self.Select(x => x.ScaleTo(newScale, initialScale));

        public static IEnumerable<Tween> ScaleTo(this IEnumerable<Transform> self, Vector3 newScale,
            Vector3? initialScale = null) => self.Select(x => x.ScaleTo(newScale, initialScale));
        #endregion


        #region Overloads
        // These methods just adds different ways to call IPrimer.ScaleTo()

        public static Tween ScaleUpFromZero(this IPrimer self, Vector3? targetScale = null)
        {
            var initial = Vector3.zero;
            var target = targetScale ?? self.GetIntrinsicScale();

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(target, initial)
                : self.component.ScaleTo(target, initial);
        }

        public static Tween ScaleDownToZero(this IPrimer self, Vector3? initialScale = null)
        {
            var initial = initialScale ?? self.GetIntrinsicScale();
            var target = Vector3.zero;

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(target, initial)
                : self.component.ScaleTo(target, initial);
        }

        public static Tween ScaleTo(this IPrimer self, float newScale, float? initialScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var initial = initialScale.HasValue
                ? Vector3.one * initialScale.Value
                : self.GetIntrinsicScale();

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(Vector3.one * newScale, initial)
                : self.component.ScaleTo(Vector3.one * newScale, initial, filePath, lineNumber);
        }

        public static Tween ScaleTo(this IPrimer self, Vector3 newScale, Vector3? initialScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var initial = initialScale ?? self.transform.localScale;

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(newScale, initial)
                : self.component.ScaleTo(newScale, initial, filePath, lineNumber);
        }
        #endregion


        #region Polyfill Component.ScaleTo()
        // These are copies of "Overloads" above but with
        // - Component instead of IPrimer

        public static Tween ScaleUpFromZero(this Component self, Vector3? targetScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var initial = Vector3.zero;
            var target = targetScale ?? self.transform.localScale;

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(target, initial)
                : self.transform.ScaleTo(target, initial, filePath, lineNumber);
        }

        public static Tween ScaleDownToZero(this Component self, Vector3? initialScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var initial = initialScale ?? self.transform.localScale;
            var target = Vector3.zero;

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(target, initial)
                : self.transform.ScaleTo(target, initial, filePath, lineNumber);
        }

        public static Tween ScaleTo(this Component self, float newScale, float? initialScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (self == null)
                return null;

            var transform = self.transform;
            
            var initial = initialScale.HasValue
                ? Vector3.one * initialScale.Value
                : transform.localScale;

            if (self is IPrimer_CustomScaleTo custom)
            {
                return custom.ScaleTo(Vector3.one * newScale, initial);
            }
            
            if (initialScale.HasValue)
                return transform.ScaleTo(Vector3.one * newScale, initial);

            return transform.ScaleTo(Vector3.one * newScale);
        }

        public static Tween ScaleTo(this Component self, Vector3 newScale, Vector3? initialScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            var transform = self.transform;
            var initial = initialScale ?? transform.localScale;

            return self is IPrimer_CustomScaleTo custom
                ? custom.ScaleTo(newScale, initial)
                : transform.ScaleTo(newScale, initial, filePath, lineNumber);
        }
        #endregion


        // Actual implementation
        // Only in Transform, all other overloads redirect here
        public static Tween ScaleTo(this Transform self, Vector3 newScale, Vector3? initialScale = null,
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (initialScale.HasValue) return new Tween(t => self.localScale = Vector3.Lerp(initialScale.Value, newScale, t));
            
            // If initialScale hasn't been given, allow the initial value to be determined later.
            return Tween.Value(
                v => self.localScale = v,
                () => self.localScale,
                () => newScale
            );
        }
    }
}
