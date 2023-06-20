using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        public static Container<TComponent> From<TComponent>(TComponent component, bool setActive = true)
            where TComponent : Component
        {
            return new Container<TComponent>(component, setActive);
        }
    }

    public partial class Container<TComponent> : Container
        where TComponent : Component
    {
        public new TComponent component { get; }

        public Container(string name, Component parent = null, bool setActive = true) : base(name, parent, setActive)
        {
            component = GetComponent<TComponent>(transform);
        }

        public Container(TComponent component, bool setActive = true) : base(component, setActive)
        {
            this.component = component;
        }

        private static TResult GetComponent<TResult>(Component source) where TResult : Component
        {
            return typeof(TResult) == typeof(Transform)
                ? source as TResult
                : source.GetComponent<TResult>()
                ?? source.gameObject.AddComponent<TResult>();
        }

        // Automatic conversion to TComponent, Component and Transform
        public static implicit operator TComponent(Container<TComponent> container) => container.component;
        public static implicit operator Component(Container<TComponent> container) => container.component;
        public static implicit operator Transform(Container<TComponent> container) => container.transform;

    }
}
