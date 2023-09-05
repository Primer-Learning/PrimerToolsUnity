using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    /// <summary>
    ///     Wrap a Unity component into a Gnome2 to manipulate it's children.
    ///     The gnome will look at the children of the component and keep track of them.
    ///     When you add a child, it will look for an unused child and use that if it exists.
    ///     After you're doing you can use .Purge() to remove all unused children.
    /// </summary>
    /// <remarks>
    ///     The idea behind this class is to be able to define the children of a GameObject in code again and again
    ///     without having to re-create them each time.
    /// </remarks>
    public partial class SimpleGnome : IDisposable, IPrimer
    {
        public Transform transform { get; }
        public Component component { get; }

        public void Dispose()
        {
            Reset();
        }
        public void Reset()
        {
            foreach (var child in transform.GetChildren())
            {
                child.gameObject.SetActive(false);
            }
            
            // created.Clear();
            // usedChildren.Clear();
            // unusedChildren.Clear();
            // unusedChildren.AddRange(ReadExistingChildren(transform));
        }

        #region Constructors
        public SimpleGnome(string name, Component parent = null)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = transform;

            Reset();
        }

        public SimpleGnome(Transform component)
        {
            this.component = component;
            transform = component;

            Reset();
        }

        protected SimpleGnome(Component component)
        {
            this.component = component;
            transform = component.transform;

            Reset();
        }

        private static Transform GetRootTransform(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : new GameObject(name);
            return obj.transform;
        }

        private static Transform GetDirectChild(Transform parent, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        public static implicit operator Transform(SimpleGnome gnome) => gnome.transform;
        #endregion
    }
}
