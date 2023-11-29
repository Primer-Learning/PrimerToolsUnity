using System;
using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface to add custom moving behavior to your IPrimer or Component.
    /// This can be used to do actions before/after moving by returning transform.MoveTo(...).Observe(...)
    /// </summary>
    public interface IPrimer_CustomMoveTo
    {
        Tween MoveTo(Vector3 newPosition, Vector3 initialPosition, bool globalSpace = false);
    }


    public static class IPrimer_MoveToExtensions
    {
        #region Overloads
        // These methods just adds different ways to call IPrimer.MoveTo()

        public static Tween MoveBy(this IPrimer self, float? x = null, float? y = null, float? z = null,
            Vector3? initialPosition = null, bool globalSpace = false)
        {
            var initial = initialPosition ?? (globalSpace ? self.transform.position : self.transform.localPosition);

            var target = new Vector3(
                x.HasValue ? initial.x + x.Value : initial.x,
                y.HasValue ? initial.y + y.Value : initial.y,
                z.HasValue ? initial.z + z.Value : initial.z
            );

            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(target, initial, globalSpace)
                : self.component.MoveTo(target, initial, globalSpace);
        }

        public static Tween MoveBy(this IPrimer self, Vector3 displacement, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            var transform = self.transform;
            var initial = initialPosition ?? (globalSpace ? transform.position : transform.localPosition);
            var target = initial + displacement;

            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(target, initial, globalSpace)
                : transform.MoveTo(target, initial, globalSpace);
        }

        public static Tween MoveTo(this IPrimer self, float? x = null, float? y = null, float? z = null,
            Vector3? initialPosition = null, bool globalSpace = false)
        {
            var initial = initialPosition ?? (globalSpace ? self.transform.position : self.transform.localPosition);

            var target = new Vector3(
                x ?? initial.x,
                y ?? initial.y,
                z ?? initial.z
            );

            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(target, initial, globalSpace)
                : self.component.MoveTo(target, initial, globalSpace);
        }

        public static Tween MoveTo(this IPrimer self, Vector3 newPosition, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            var initial = initialPosition ?? (globalSpace ? self.transform.position : self.transform.localPosition);

            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(newPosition, initial, globalSpace)
                : self.component.MoveTo(newPosition, initial, globalSpace);
        }
        #endregion


        #region Polyfill Component.MoveTo()
        // These are copies of "Overloads" above but with
        // - Component instead of IPrimer
        // - fallback to transform instead of self.component

        public static Tween MoveBy(this Component self, float? x = null, float? y = null, float? z = null,
            Vector3? initialPosition = null, bool globalSpace = false)
        {
            var transform = self.transform;
            var initial = initialPosition ?? (globalSpace ? transform.position : transform.localPosition);

            var target = new Vector3(
                x.HasValue ? initial.x + x.Value : initial.x,
                y.HasValue ? initial.y + y.Value : initial.y,
                z.HasValue ? initial.z + z.Value : initial.z
            );

            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(target, initial, globalSpace)
                : transform.MoveTo(target, initial, globalSpace);
        }

        public static Tween MoveBy(this Component self, Vector3 displacement, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            if (globalSpace)
                return Tween.Value(
                    v => self.transform.position = v,
                    () => self.transform.position,
                    () => self.transform.position + displacement
                );
            
            return Tween.Value(
                v => self.transform.localPosition = v,
                () => self.transform.localPosition,
                () => self.transform.localPosition + displacement
            );
        }

        public static Tween MoveTo(this Component self, float? x = null, float? y = null, float? z = null,
            Vector3? initialPosition = null, bool globalSpace = false)
        {
            var transform = self.transform;
            var initial = initialPosition ?? (globalSpace ? transform.position : transform.localPosition);

            var target = new Vector3(
                x ?? initial.x,
                y ?? initial.y,
                z ?? initial.z
            );

            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(target, initial, globalSpace)
                : transform.MoveTo(target, initial, globalSpace);
        }

        public static Tween MoveTo(this Component self, Vector3 newPosition, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            var transform = self.transform;
            var initial = initialPosition ?? (globalSpace ? transform.position : transform.localPosition);
            
            return self is IPrimer_CustomMoveTo custom
                ? custom.MoveTo(newPosition, initial, globalSpace)
                : transform.MoveTo(newPosition, globalSpace);
        }
        #endregion

        // Main implementation
        // Only in Transform, all other overloads redirect here
        // Uses Tween.Value instead of new Tween so the initial position is dynamic (rarely do you want to set it manually)
        // If you do want to set it manually, MoveTo won't work for you. Use the regular Tween constructor instead.
        public static Tween MoveTo(this Transform self, Vector3 newPosition,
            bool globalSpace = false)
        {
            if (globalSpace)
                return Tween.Value(
                    v => self.position = v,
                    () => self.position,
                    () => newPosition
                );
            
            return Tween.Value(
                v => self.localPosition = v,
                () => self.localPosition,
                () => newPosition
            );
        }
        
        // This one lets the destination and duration also be dynamic
        // For cases where many tweens are defined before they are evaluated
        // Could make a bunch of overloads depending which parameters are dynamic, but seems cleaner to do one.
        public static Tween MoveToDynamic(this Transform self, Func<Vector3> to, Func<float> durationFunc,
            bool globalSpace = false)
        {
            if (globalSpace)
                return Tween.Value(
                    v => self.position = v,
                    () => self.position,
                    to,
                    durationFunc
                );
            
            return Tween.Value(
                v => self.localPosition = v,
                () => self.localPosition,
                to,
                durationFunc
            );
        }
    }
}
