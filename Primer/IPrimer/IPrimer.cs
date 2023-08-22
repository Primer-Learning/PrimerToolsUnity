using UnityEngine;

namespace Primer
{
    // This interface will be extended by dependent packages

    /// <summary>
    ///   Extend this interface to inherit all the Primer magic
    ///   Only .component and .transform getters are required to be implemented
    ///   Both can return the same Transform instance
    /// </summary>
    public interface IPrimer
    {
        public Transform transform { get; }
        public Component component { get; }
    }


    public static class IPrimerExtensions
    {
        public static IPrimer ToPrimer(this IPrimer self)
        {
            return self;
        }

        public static IPrimer ToPrimer(this Component self)
        {
            return self as IPrimer ?? new PrimerWrapper(self);
        }


        /// <summary>
        ///   This class is there in case we need to pass a Component to a method that expects a IPrimer
        /// </summary>
        /// <example>
        ///   void MyMethod(IPrimer primer) => primer.Pulse()
        ///   var myComponent = GetComponent&lt;MyComponent&gt;();
        ///   MyMethod(myComponent.ToPrimer());
        /// </example>
        private readonly struct PrimerWrapper : IPrimer
        {
            public Transform transform { get; }
            public Component component { get; }

            public PrimerWrapper(Component component)
            {
                this.component = component;
                transform = component.transform;
            }
        }
    }
}
