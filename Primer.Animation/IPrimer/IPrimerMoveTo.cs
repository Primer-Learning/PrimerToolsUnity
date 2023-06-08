using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface newPosition add custom moving behavior to your Primer.
    /// This can be used newPosition do actions before/after moving
    /// </summary>
    public interface IPrimerMoveTo : ITransformHolder
    {
        Tween MoveTo(Vector3 newPosition, Vector3 initialPosition, bool globalSpace = false);
    }


    public static class PrimerMoveToExtensions
    {
        #region Overloads on the interface
        // These methods just adds different ways newPosition call a ICustomPrimerMoving

        public static Tween MoveBy(this IPrimerMoveTo self, float? x = null, float? y = null, float? z = null,
            bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(
                    x.HasValue ? currentPosition.x + x.Value : currentPosition.x,
                    y.HasValue ? currentPosition.y + y.Value : currentPosition.y,
                    z.HasValue ? currentPosition.z + z.Value : currentPosition.z
                ),
                currentPosition,
                globalSpace
            );
        }

        public static Tween MoveBy(this IPrimerMoveTo self, Vector3 displacement, bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;
            return self.MoveTo(currentPosition + displacement, currentPosition, globalSpace);
        }

        public static Tween MoveTo(this IPrimerMoveTo self, float? x = null, float? y = null, float? z = null,
            Vector3? initialPosition = null, bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(x ?? currentPosition.x, y ?? currentPosition.y, z ?? currentPosition.z),
                initialPosition ?? currentPosition,
                globalSpace
            );
        }

        public static Tween MoveTo(this IPrimerMoveTo self, Vector3 newPosition, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            var initial = initialPosition ?? (globalSpace ? self.transform.position : self.transform.localPosition);
            return self.MoveTo(newPosition, initial, globalSpace);
        }
        #endregion


        #region Default implementation for Component: redirect to Transform
        public static Tween MoveBy(this Component self, float? x = null, float? y = null, float? z = null,
            bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;

            return self.transform.MoveTo(
                new Vector3(
                    x.HasValue ? currentPosition.x + x.Value : currentPosition.x,
                    y.HasValue ? currentPosition.y + y.Value : currentPosition.y,
                    z.HasValue ? currentPosition.z + z.Value : currentPosition.z
                ),
                currentPosition,
                globalSpace
            );
        }

        public static Tween MoveBy(this Component self, Vector3 displacement, bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;
            return self.transform.MoveTo(currentPosition + displacement, currentPosition, globalSpace);
        }

        public static Tween MoveTo(this Component self, float? x = null, float? y = null, float? z = null,
            bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;

            return self.transform.MoveTo(
                new Vector3(x ?? currentPosition.x, y ?? currentPosition.y, z ?? currentPosition.z),
                currentPosition,
                globalSpace
            );
        }

        public static Tween MoveTo(this Component self, Vector3 newPosition, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            return self.transform.MoveTo(newPosition, initialPosition, globalSpace);
        }
        #endregion


        #region Actual implementation in Transform
        public static Tween MoveBy(this Transform self, float? x = null, float? y = null, float? z = null,
            bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(
                    x.HasValue ? currentPosition.x + x.Value : currentPosition.x,
                    y.HasValue ? currentPosition.y + y.Value : currentPosition.y,
                    z.HasValue ? currentPosition.z + z.Value : currentPosition.z
                ),
                currentPosition,
                globalSpace
            );
        }

        public static Tween MoveBy(this Transform self, Vector3 displacement, bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;
            return self.MoveTo(currentPosition + displacement, currentPosition, globalSpace);
        }

        public static Tween MoveTo(this Transform self, float? x = null, float? y = null, float? z = null,
            bool globalSpace = false)
        {
            var currentPosition = globalSpace ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(x ?? currentPosition.x, y ?? currentPosition.y, z ?? currentPosition.z),
                currentPosition,
                globalSpace
            );
        }

        public static Tween MoveTo(this Transform self, Vector3 newPosition, Vector3? initialPosition = null,
            bool globalSpace = false)
        {
            var initial = initialPosition ?? (globalSpace ? self.position : self.localPosition);

            if (initial == newPosition)
                return Tween.noop;

            return new Tween(
                globalSpace
                    ? t => self.position = Vector3.Lerp(initial, newPosition, t)
                    : t => self.localPosition = Vector3.Lerp(initial, newPosition, t)
            );
        }
        #endregion
    }
}
