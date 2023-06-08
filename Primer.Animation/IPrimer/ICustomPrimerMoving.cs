using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    /// Implement this interface newPosition add custom moving behavior to your Primer.
    /// This can be used newPosition do actions before/after moving
    /// </summary>
    public interface ICustomPrimerMoving : ITransformHolder
    {
        // TODO: rename useGlobalPosition newPosition globalSpace
        Tween MoveTo(Vector3 newPosition, Vector3 initialPosition, bool useGlobalPosition = false);
    }


    public static class CustomPrimerMovingExtensions
    {
        #region Overloads on the interface
        // These methods just adds different ways newPosition call a ICustomPrimerMoving

        public static Tween MoveBy(this ICustomPrimerMoving self, float? x = null, float? y = null, float? z = null,
            bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(
                    x.HasValue ? currentPosition.x + x.Value : currentPosition.x,
                    y.HasValue ? currentPosition.y + y.Value : currentPosition.y,
                    z.HasValue ? currentPosition.z + z.Value : currentPosition.z
                ),
                currentPosition,
                useGlobalPosition
            );
        }

        public static Tween MoveBy(this ICustomPrimerMoving self, Vector3 displacement, bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;
            return self.MoveTo(currentPosition + displacement, currentPosition, useGlobalPosition);
        }

        public static Tween MoveTo(this ICustomPrimerMoving self, float? x = null, float? y = null, float? z = null,
            Vector3? initialPosition = null, bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(x ?? currentPosition.x, y ?? currentPosition.y, z ?? currentPosition.z),
                initialPosition ?? currentPosition,
                useGlobalPosition
            );
        }

        public static Tween MoveTo(this ICustomPrimerMoving self, Vector3 newPosition, Vector3? initialPosition = null,
            bool useGlobalPosition = false)
        {
            var initial = initialPosition ?? (useGlobalPosition ? self.transform.position : self.transform.localPosition);
            return self.MoveTo(newPosition, initial, useGlobalPosition);
        }
        #endregion


        #region Default implementation for Component: redirect to Transform
        public static Tween MoveBy(this Component self, float? x = null, float? y = null, float? z = null,
            bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;

            return self.transform.MoveTo(
                new Vector3(
                    x.HasValue ? currentPosition.x + x.Value : currentPosition.x,
                    y.HasValue ? currentPosition.y + y.Value : currentPosition.y,
                    z.HasValue ? currentPosition.z + z.Value : currentPosition.z
                ),
                currentPosition,
                useGlobalPosition
            );
        }

        public static Tween MoveBy(this Component self, Vector3 displacement, bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;
            return self.transform.MoveTo(currentPosition + displacement, currentPosition, useGlobalPosition);
        }

        public static Tween MoveTo(this Component self, float? x = null, float? y = null, float? z = null,
            bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;

            return self.transform.MoveTo(
                new Vector3(x ?? currentPosition.x, y ?? currentPosition.y, z ?? currentPosition.z),
                currentPosition,
                useGlobalPosition
            );
        }

        public static Tween MoveTo(this Component self, Vector3 newPosition, Vector3? initialPosition = null,
            bool useGlobalPosition = false)
        {
            return self.transform.MoveTo(newPosition, initialPosition, useGlobalPosition);
        }
        #endregion


        #region Actual implementation in Transform
        public static Tween MoveBy(this Transform self, float? x = null, float? y = null, float? z = null,
            bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(
                    x.HasValue ? currentPosition.x + x.Value : currentPosition.x,
                    y.HasValue ? currentPosition.y + y.Value : currentPosition.y,
                    z.HasValue ? currentPosition.z + z.Value : currentPosition.z
                ),
                currentPosition,
                useGlobalPosition
            );
        }

        public static Tween MoveBy(this Transform self, Vector3 displacement, bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;
            return self.MoveTo(currentPosition + displacement, currentPosition, useGlobalPosition);
        }

        public static Tween MoveTo(this Transform self, float? x = null, float? y = null, float? z = null,
            bool useGlobalPosition = false)
        {
            var currentPosition = useGlobalPosition ? self.transform.position : self.transform.localPosition;

            return self.MoveTo(
                new Vector3(x ?? currentPosition.x, y ?? currentPosition.y, z ?? currentPosition.z),
                currentPosition,
                useGlobalPosition
            );
        }

        public static Tween MoveTo(this Transform self, Vector3 newPosition, Vector3? initialPosition = null,
            bool useGlobalPosition = false)
        {
            var initial = initialPosition ?? (useGlobalPosition ? self.position : self.localPosition);

            if (initial == newPosition)
                return Tween.noop;

            return new Tween(
                useGlobalPosition
                    ? t => self.position = Vector3.Lerp(initial, newPosition, t)
                    : t => self.localPosition = Vector3.Lerp(initial, newPosition, t)
            );
        }
        #endregion
    }
}
