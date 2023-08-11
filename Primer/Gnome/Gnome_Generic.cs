using UnityEngine;

namespace Primer
{
    public partial class Gnome
    {
        public static Gnome<TComponent> For<TComponent>(TComponent component, bool setActive = true)
            where TComponent : Component
        {
            return new Gnome<TComponent>(component, setActive);
        }
    }

    public partial class Gnome<TComponent> : Gnome
        where TComponent : Component
    {
        public new TComponent component { get; }

        public Gnome(string name, Component parent = null, bool setActive = true) : base(name, parent, setActive)
        {
            component = GetComponent<TComponent>(transform);
        }

        public Gnome(TComponent component, bool setActive = true) : base(component, setActive)
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
        public static implicit operator TComponent(Gnome<TComponent> gnome) => gnome.component;
        public static implicit operator Component(Gnome<TComponent> gnome) => gnome.component;
        public static implicit operator Transform(Gnome<TComponent> gnome) => gnome.transform;

    }
}
