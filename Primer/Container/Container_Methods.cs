using System;
using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {

        public void Insert<TChild>(TChild child, bool worldPositionStays = false, int? index = null)
            where TChild : Component
        {
            var position = index ?? childCount;
            var t = child.transform;

            t.SetActive(true);

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

        public void OnCleanup(Action action)
        {
            onCleanup.Add(action);
        }

        public void Dispose()
        {
            if (transform != null)
                transform.SetActive(false);

            foreach (var broomUp in onCleanup)
                broomUp();
        }
    }
}
