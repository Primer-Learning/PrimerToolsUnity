using UnityEngine;

namespace Primer.Animation
{
    public interface IPrimer
        : ICustomPrimerMoving, ICustomPrimerRotation, ICustomPrimerScaling
    {
    }


    public static class PrimerExtensions
    {
        #region anything.ToPrimer()
        public static IPrimer ToPrimer(this IPrimer self)
        {
            return self;
        }

        public static IPrimer ToPrimer<T>(this Container<T> self) where T : Component
        {
            return new PrimerTransformWrapper(self.component);
        }

        public static IPrimer ToPrimer(this Component self)
        {
            return new PrimerTransformWrapper(self);
        }

        public static IPrimer ToPrimer(this Transform self)
        {
            return new PrimerTransformWrapper(self);
        }
        #endregion

        #region MoveAndRotate()
        public static Tween MoveAndRotate(this IPrimer self, Vector3 newPosition, Quaternion newRotation)
        {
            return Tween.Parallel(
                self.MoveTo(newPosition, newPosition),
                self.RotateTo(newRotation)
            );
        }

        public static Tween MoveAndRotate(this Component self, Vector3 newPosition, Quaternion newRotation)
        {
            return Tween.Parallel(
                self.MoveTo(newPosition, newPosition),
                self.RotateTo(newRotation)
            );
        }
        #endregion


        private readonly struct PrimerTransformWrapper : IPrimer
        {
            private readonly Component component;
            public Transform transform { get; }

            public PrimerTransformWrapper(Component component)
            {
                this.component = component;
                transform = component.transform;
            }

            public Tween MoveTo(Vector3 to, Vector3 initial, bool useGlobalPosition = false)
            {
                // Sh  resharper... there will be implementations of this interface
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (component is ICustomPrimerMoving custom)
                    // This is a specific method implemented in-class
                    return custom.MoveTo(to, initial, useGlobalPosition);

                // This is a generic extension method
                return transform.MoveTo(to, initial, useGlobalPosition);
            }

            public Tween RotateTo(Quaternion to, Quaternion initial)
            {
                // Sh  resharper... there will be implementations of this interface
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (component is ICustomPrimerRotation custom)
                    // This is a specific method implemented in-class
                    return custom.RotateTo(to, initial);

                // This is a generic extension method
                return transform.RotateTo(to, initial);
            }

            public Tween ScaleTo(Vector3 to, Vector3 initial)
            {
                // Sh  resharper... there will be implementations of this interface
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (component is ICustomPrimerScaling custom)
                    // This is a specific method implemented in-class
                    return custom.ScaleTo(to, initial);

                // This is a generic extension method
                return transform.ScaleTo(to, initial);
            }
        }
    }
}
