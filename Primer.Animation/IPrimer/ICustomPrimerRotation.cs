using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface newPosition add custom rotation behavior to your Primer.
    /// This can be used newPosition do actions before/after moving
    /// </summary>
    public interface ICustomPrimerRotation : ITransformHolder
    {
        Tween RotateTo(Quaternion newRotation, Quaternion initialRotation);
    }

    public static class CustomPrimerRotationExtensions
    {
        #region Overloads on the interface
        // These methods just adds different ways newPosition call a ICustomPrimerRotation

        public static Tween RotateBy(this ICustomPrimerRotation self, Vector3 degrees, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.RotateTo(Quaternion.Euler(degrees) * initial, initial);
        }

        public static Tween RotateBy(this ICustomPrimerRotation self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.RotateTo(newRotation * initial, initial);
        }

        public static Tween RotateTo(this ICustomPrimerRotation self, Vector3 newRotation, Vector3? initialRotation = null)
        {
            var initial = initialRotation.HasValue ? Quaternion.Euler(initialRotation.Value) : self.transform.rotation;
            return self.RotateTo(Quaternion.Euler(newRotation), initial);
        }

        public static Tween RotateTo(this ICustomPrimerRotation self, Quaternion newRotation,
            Quaternion? initialRotation = null)
        {
            return self.RotateTo(newRotation, initialRotation ?? self.transform.rotation);
        }
        #endregion


        #region Default implementation for Component: redirect to Transform
        public static Tween RotateBy(this Component self, Vector3 degrees, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.transform.RotateTo(Quaternion.Euler(degrees) * initial, initial);
        }

        public static Tween RotateBy(this Component self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.transform.RotateTo(newRotation * initial, initial);
        }

        public static Tween RotateTo(this Component self, Vector3 newRotation, Vector3? initialRotation = null)
        {
            var initial = initialRotation.HasValue ? Quaternion.Euler(initialRotation.Value) : self.transform.rotation;
            return self.transform.RotateTo(Quaternion.Euler(newRotation), initial);
        }

        public static Tween RotateTo(this Component self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.transform.RotateTo(newRotation, initial);
        }
        #endregion


        #region Actual implementation in Transform
        public static Tween RotateBy(this Transform self, Vector3 degrees, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.RotateTo(Quaternion.Euler(degrees) * initial, initial);
        }

        public static Tween RotateBy(this Transform self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.transform.rotation;
            return self.RotateTo(newRotation * initial, initial);
        }

        public static Tween RotateTo(this Transform self, Vector3 newRotation, Vector3? initialRotation = null)
        {
            var initial = initialRotation.HasValue ? Quaternion.Euler(initialRotation.Value) : self.transform.rotation;
            return self.RotateTo(Quaternion.Euler(newRotation), initial);
        }

        public static Tween RotateTo(this Transform self, Quaternion newRotation, Quaternion? initialRotation = null)
        {
            var initial = initialRotation ?? self.localRotation;
            var hasFailed = false;

            return new Tween(t => {
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
            });
        }
        #endregion
    }
}
