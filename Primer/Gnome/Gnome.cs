using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public partial class Gnome : IDisposable, IPrimer
    {
        public Transform transform { get; }
        public Component component { get; }

        public Gnome(string name, Component parent = null, bool setActive = true)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = transform;

            if (setActive)
                transform.SetActive(true);

            Reset();
        }

        public Gnome(Transform component, bool setActive = true)
        {
            this.component = component;
            transform = component;

            if (setActive)
                transform.SetActive(true);

            Reset();
        }

        protected Gnome(Component component, bool setActive = true)
        {
            this.component = component;
            transform = component.transform;

            if (setActive)
                transform.SetActive(true);

            Reset();
        }


        public void Reset()
        {
            usedChildren.Clear();
            unusedChildren.Clear();
            unusedChildren.AddRange(ReadExistingChildren(transform));
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

        public static implicit operator Transform(Gnome gnome) => gnome.transform;
    }
}
