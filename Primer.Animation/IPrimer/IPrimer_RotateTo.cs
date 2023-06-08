using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface newPosition add custom rotation behavior to your IPrimer or Component.
    /// This can be used to do actions before/after rotating by returning transform.RotateTo(...).Observe(...)
    /// </summary>
    public interface IPrimer_CustomRotateTo
    {
        Tween RotateTo(Quaternion newRotation, Quaternion initialRotation);
    }

    public static class IPrimer_RotateToExtensions
    {
        #region Overloads
        // These methods just adds different ways to call IPrimer.RotateTo()

        public static Tween RotateBy(this IPrimer self, float x = 0, float y = 0, float z = 0, Quaternion? initialRotation = null)
        {
            var transform = self.component.transform;
            var initial = initialRotation ?? transform.rotation;
            var target = Quaternion.Euler(x, y, z) * initial;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateBy(this IPrimer self, Vector3 degrees, Quaternion? initialRotation = null)
        {
            var transform = self.component.transform;
            var initial = initialRotation ?? transform.rotation;
            var target = Quaternion.Euler(degrees) * initial;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateBy(this IPrimer self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var transform = self.component.transform;
            var initial = initialRotation ?? transform.rotation;
            var target = newRotation * initial;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateTo(this IPrimer self, Vector3 newRotation, Vector3? initialRotation = null)
        {
            var transform = self.component.transform;

            var initial = initialRotation.HasValue
                ? Quaternion.Euler(initialRotation.Value)
                : transform.rotation;

            var target = Quaternion.Euler(newRotation);

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateTo(this IPrimer self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var transform = self.component.transform;
            var initial = initialRotation ?? transform.rotation;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(newRotation, initial)
                : transform.RotateTo(newRotation, initial);
        }
        #endregion


        #region Polyfill Component.RotateTo()
        // These are copies of "Overloads" above but with
        // - Component instead of IPrimer
        // - self.transform instead of self.component.transform

        public static Tween RotateBy(this Component self, float x = 0, float y = 0, float z = 0, Quaternion? initialRotation = null)
        {
            var transform = self.transform;
            var initial = initialRotation ?? transform.rotation;
            var target = Quaternion.Euler(x, y, z) * initial;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateBy(this Component self, Vector3 degrees, Quaternion? initialRotation = null)
        {
            var transform = self.transform;
            var initial = initialRotation ?? transform.rotation;
            var target = Quaternion.Euler(degrees) * initial;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateBy(this Component self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var transform = self.transform;
            var initial = initialRotation ?? transform.rotation;
            var target = newRotation * initial;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateTo(this Component self, Vector3 newRotation, Vector3? initialRotation = null)
        {
            var transform = self.transform;

            var initial = initialRotation.HasValue
                ? Quaternion.Euler(initialRotation.Value)
                : transform.rotation;

            var target = Quaternion.Euler(newRotation);

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(target, initial)
                : transform.RotateTo(target, initial);
        }

        public static Tween RotateTo(this Component self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var transform = self.transform;
            var initial = initialRotation ?? transform.rotation;

            return self is IPrimer_CustomRotateTo custom
                ? custom.RotateTo(newRotation, initial)
                : transform.RotateTo(newRotation, initial);
        }
        #endregion


        // Actual implementation
        // Only in Transform, all other overloads redirect here
        public static Tween RotateTo(this Transform self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.localRotation;
            var hasFailed = false;

            return new Tween(
                t => {
                    if (hasFailed)
                        return;

                    try {
                        self.localRotation = Quaternion.Lerp(initial, newRotation, t);
                    }
                    catch {
                        // GPT 4 says quaternion equality check is unreliable, and that Unity does not allow lerping of quaternions
                        // that are very close together, for some reason.
                        Debug.LogWarning("Tween failed in RotateTo. Quaternions may be too close.");
                        hasFailed = true;
                        self.localRotation = newRotation;
                    }
                }
            );
        }
    }
}
