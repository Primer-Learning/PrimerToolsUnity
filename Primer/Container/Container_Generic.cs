using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        public static Container<TComponent> From<TComponent>(TComponent component)
            where TComponent : Component
        {
            return new Container<TComponent>(component);
        }
    }

    public partial class Container<TComponent> : Container
        where TComponent : Component
    {
        // public static Container<T> From<T>(T component) where T : Component => new(component);

        public new TComponent component { get; }

        public Container(string name, Component parent = null) : base(name, parent)
        {
            component = GetComponent<TComponent>(transform);
        }

        public Container(TComponent component) : base(component)
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

        public static Container<T> From<T>(T component) where T : Component => new(component);

        // Automatic conversion to TComponent, Component and Transform
        public static implicit operator TComponent(Container<TComponent> container) => container.component;
        public static implicit operator Component(Container<TComponent> container) => container.component;
        public static implicit operator Transform(Container<TComponent> container) => container.transform;

    }
}
