using System;
using UnityEngine;

namespace Primer
{
    // This part contains the methods to define new children
    public partial class Gnome
    {
        public Transform Add(string name = null, ChildOptions options = null)
        {
            var child = FindChild<Transform>(name) ?? OnCreate(CreateObject(name).transform);
            Insert(child, options);
            return child;
        }

        public TChild Add<TChild>(string name = null, ChildOptions options = null)
            where TChild : Component
        {
            var child = FindChild<TChild>(name) ?? OnCreate(CreateObject(name).AddComponent<TChild>());
            Insert(child, options);
            return child;
        }

        public TChild Add<TChild>(TChild template, string name = null, ChildOptions options = null)
            where TChild : Component
        {
            var child = FindInstanceOf(template, name)
                ?? OnCreate(InstantiationTracker.InstantiateAndRegister(template, name));

            Insert(child, options);
            return child;
        }

        public TChild Add<TChild>(PrefabProvider<TChild> provider, string name = null, ChildOptions options = null)
            where TChild : Component
        {
            if (provider.value == null)
                throw new ArgumentNullException(nameof(provider), "PrefabProvider value is null.");

            var child = FindInstanceOf(provider.value, name)
                ?? OnCreate(InstantiationTracker.InstantiateAndRegister(provider.value, name));

            Insert(child, options);
            provider.Initialize(child);
            return child;
        }

        private GameObject CreateObject(string name)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            return go;
        }

        private TChild FindChild<TChild>(string childName) where TChild : Component
        {
            var hasName = !string.IsNullOrWhiteSpace(childName);
            var isTransform = typeof(TChild) == typeof(Transform);

            for (var i = 0; i < unusedChildren.Count; i++) {
                var child = unusedChildren[i];

                if (!child || hasName && child.name != childName)
                    continue;

                if (isTransform)
                    return child as TChild;

                if (child.TryGetComponent<TChild>(out var childComponent))
                    return childComponent;
            }

            return null;
        }

        private TChild FindInstanceOf<TChild>(TChild template, string childName) where TChild : Component
        {
            var hasName = !string.IsNullOrWhiteSpace(childName);
            var isTransform = typeof(TChild) == typeof(Transform);

            for (var i = 0; i < unusedChildren.Count; i++) {
                var child = unusedChildren[i];

                if (!child || hasName && child.name != childName)
                    continue;

                if (!InstantiationTracker.IsInstanceOf(child, template))
                    continue;

                return isTransform ? child as TChild : child.GetComponent<TChild>();
            }

            return null;
        }
    }
}