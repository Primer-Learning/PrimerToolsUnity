using UnityEngine;

namespace Primer.Animation
{
    /// <summary>
    ///   Extend this class to inherit all the Primer magic
    ///   Only a small amount of methods are required to be implemented and will be enforced by the compiler
    /// </summary>
    public interface IPrimer
        : IPrimerMoveTo, IPrimerRotateTo, IPrimerScaleTo
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


        /// <summary>
        ///   This class is there in case we need to pass a Component to a method that expects a IPrimer
        /// </summary>
        /// <example>
        ///   void MyMethod(IPrimer primer) => primer.Pulse()
        ///   var myComponent = GetComponent<MyComponent>();
        ///   MyMethod(myComponent.ToPrimer());
        /// </example>
        private readonly struct PrimerTransformWrapper : IPrimer
        {
            private readonly Component component;
            public Transform transform { get; }

            public PrimerTransformWrapper(Component component)
            {
                this.component = component;
                transform = component.transform;
            }

            public Tween MoveTo(Vector3 to, Vector3 initial, bool globalSpace = false)
            {
                // Sh  resharper... there will be implementations of this interface
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (component is IPrimerMoveTo custom)
                    // This is a specific method implemented in-class
                    return custom.MoveTo(to, initial, globalSpace);

                // This is a generic extension method
                return transform.MoveTo(to, initial, globalSpace);
            }

            public Tween RotateTo(Quaternion to, Quaternion initial)
            {
                // Sh  resharper... there will be implementations of this interface
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (component is IPrimerRotateTo custom)
                    // This is a specific method implemented in-class
                    return custom.RotateTo(to, initial);

                // This is a generic extension method
                return transform.RotateTo(to, initial);
            }

            public Tween ScaleTo(Vector3 to, Vector3 initial)
            {
                // Sh  resharper... there will be implementations of this interface
                // ReSharper disable once SuspiciousTypeConversion.Global
                if (component is IPrimerScaleTo custom)
                    // This is a specific method implemented in-class
                    return custom.ScaleTo(to, initial);

                // This is a generic extension method
                return transform.ScaleTo(to, initial);
            }
        }
    }
}
