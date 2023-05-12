using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public class Container : Container<Transform>
    {
        public Container(string name) : base(name) {}
        public Container(Transform t) : base(t) {}
    }

    public class Container<TComponent> : IDisposable
        where TComponent : Component
    {
        public int childCount => usedChildren.Count;
        public Transform transform { get; }
        public TComponent component { get; }

        public Container(string name, Component parent = null)
        {
            transform = parent is null
                ? GetRootTransform(name)
                : GetDirectChild(parent.transform, name);

            component = GetComponent<TComponent>(transform);
            unusedChildren = transform.GetChildren().ToList();
            transform.SetActive(true);
        }

        public Container(TComponent component)
        {
            this.component = component;
            transform = component.transform;
            unusedChildren = transform.GetChildren().ToList();
            transform.SetActive(true);
        }

        public void Insert<TChild>(TChild child, bool worldPositionStays = false, int? index = null)
            where TChild : Component
        {
            var position = index ?? childCount;
            var t = child.transform;

            if (t.parent != transform)
                t.SetParent(transform, worldPositionStays);

            if (t.GetSiblingIndex() != position)
                t.SetSiblingIndex(position);

            if (index.HasValue)
                usedChildren.Insert(index.Value, t);
            else
                usedChildren.Add(t);

            unusedChildren.Remove(t);
        }

        public Transform Add(string name = null, bool worldPositionStays = false)
        {
            var child = FindChild<Transform>(name) ?? new GameObject(name).transform;
            Insert(child, worldPositionStays);
            return child;
        }

        public TChild Add<TChild>(string name = null, bool worldPositionStays = false) where TChild : Component
        {
            var child = FindChild<TChild>(name) ?? new GameObject(name).AddComponent<TChild>();
            Insert(child, worldPositionStays);
            return child;
        }

        public TChild Add<TChild>(TChild template, string name = null, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = FindInstanceOf(template, name) ?? InstantiationTracker.InstantiateAndRegister(template, name);
            Insert(child, worldPositionStays);
            return child;
        }

        public Container AddContainer(string name, bool worldPositionStays = false)
        {
            var child = Add(name, worldPositionStays);
            var childContainer = new Container(child);
            onPurge.Add(() => childContainer.Purge());
            return childContainer;
        }

        public Container<TChild> AddContainer<TChild>(string name, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = Add<TChild>(name, worldPositionStays);
            var childContainer = new Container<TChild>(child);
            onPurge.Add(() => childContainer.Purge());
            return childContainer;
        }

        public Container<TChild> AddContainer<TChild>(TChild template, string name, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = Add(template, name, worldPositionStays);
            var childContainer = new Container<TChild>(child);
            onPurge.Add(() => childContainer.Purge());
            return childContainer;
        }

        public void Purge()
        {
            foreach (var child in unusedChildren)
                child.Dispose();

            foreach (var listener in onPurge)
                listener();

            unusedChildren.Clear();
        }

        public void Dispose()
        {
            if (transform != null)
                transform.SetActive(false);
        }


        #region Internals
        private readonly List<Transform> usedChildren = new();
        private readonly List<Transform> unusedChildren;
        private readonly List<Action> onPurge = new();

        private TChild FindChild<TChild>(string childName) where TChild : Component
        {
            var hasName = !string.IsNullOrWhiteSpace(childName);
            var isTransform = typeof(TChild) == typeof(Transform);

            for (var i = 0; i < unusedChildren.Count; i++) {
                var child = unusedChildren[i];

                if (hasName && child.name != childName)
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

                if (hasName && child.name != childName)
                    continue;

                if (!InstantiationTracker.IsInstanceOf(child, template))
                    continue;

                return isTransform ? child as TChild : child.GetComponent<TChild>();
            }

            return null;
        }

        private static Transform GetRootTransform(string name)
        {
            var obj = SceneManager.GetActiveScene()
                    .GetRootGameObjects()
                    .FirstOrDefault(x => x.name == name)
                ?? new GameObject(name);

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

        private static TResult GetComponent<TResult>(Component source) where TResult : Component
        {
            return typeof(TResult) == typeof(Transform)
                ? source as TResult
                : source.GetComponent<TResult>()
                ?? source.gameObject.AddComponent<TResult>();
        }
        #endregion
    }
}
