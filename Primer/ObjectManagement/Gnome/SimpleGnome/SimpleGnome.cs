using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    /// <summary>
    ///     Wrap a Unity component into a Gnome to manipulate it's children.
    ///     The gnome will look at the children of the component and keep track of them.
    ///     When you add a child, it will look for an unused child and use that if it exists.
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
        
        public IEnumerable<Transform> activeChildren => transform.GetChildren().Where(x => x.gameObject.activeSelf);
        public int activeChildCount => transform.GetChildren().Count(x => x.gameObject.activeSelf);

        #region Constructors
        public SimpleGnome(string name, Component parent = null)
        {
            transform = parent is null
                ? ObjectManagementUtilities.GetRootTransform(name)
                : ObjectManagementUtilities.GetDirectChild(parent.transform, name);

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
                ? ObjectManagementUtilities.GetRootTransform(prefab, name)
                : ObjectManagementUtilities.GetDirectChild(parent.transform, prefab, name);

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
        public static implicit operator Transform(SimpleGnome gnome) => gnome.transform;
    }
}
