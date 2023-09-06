using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

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
            transform.gameObject.SetActive(false);
        }
        public void Reset(bool hard = false)
        {
            foreach (var child in transform.GetChildren())
            {
                if (hard)
                    child.gameObject.Dispose();
                else
                    child.gameObject.SetActive(false);
            }
        }
        
        public int activeChildCount => transform.GetChildren().Count(x => x.gameObject.activeSelf);

        #region Constructors
        public SimpleGnome(string name, Component parent = null)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = transform;
            transform.gameObject.SetActive(true);
        }

        public SimpleGnome(Transform component)
        {
            this.component = component;
            transform = component;
            transform.gameObject.SetActive(true);
        }

        protected SimpleGnome(Component component)
        {
            this.component = component;
            transform = component.transform;
            transform.gameObject.SetActive(true);
        }
        #endregion
        
        #region Constructors that take a prefab
        // Create a SimpleGnome from a prefab. Mostly useful for root objects you want to disable themselves.
        public SimpleGnome(GameObject prefab, string name, Transform parent = null)
        {
            transform = parent is null
                ? GetRootTransform(prefab, name)
                : GetDirectChild(parent.transform, prefab, name);

            transform.name = name;
            component = transform;
            transform.gameObject.SetActive(true);
        }
        
        // Overload for the above that takes a prefab name instead of a prefab.
        public SimpleGnome(string prefabName, string name, Transform parent = null)
        : this(Resources.Load<GameObject>(prefabName), name, parent)
        {
        }
        #endregion

        #region Utilities
        // Gets or creates a root object with the given name.
        private static Transform GetRootTransform(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : new GameObject(name);
            return obj.transform;
        }
        
        // Overload for the above that takes a prefab
        private static Transform GetRootTransform(GameObject prefab, string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : Object.Instantiate(prefab);
            obj.name = name;
            return obj.transform;
        }

        // Gets or creates a child object with the given name.
        private static Transform GetDirectChild(Transform parent, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }
        
        // Overload for the above that takes a prefab
        private static Transform GetDirectChild(Transform parent, GameObject prefab, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = Object.Instantiate(prefab).transform;
            child.SetParent(parent, false);
            return child;
        }

        public static implicit operator Transform(SimpleGnome gnome) => gnome.transform;
        #endregion
    }
}
