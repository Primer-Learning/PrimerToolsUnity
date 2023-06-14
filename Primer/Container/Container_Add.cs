using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        private readonly List<Transform> usedChildren = new();
        private readonly List<Transform> unusedChildren = new();

        public int childCount => usedChildren.Count;

        public void Insert<TChild>(TChild child, bool worldPositionStays = false, int? index = null)
            where TChild : Component
        {
            var siblingIndex = index ?? childCount;
            var t = child.transform;

            t.SetActive(true);

            if (t.parent != transform)
                t.SetParent(transform, worldPositionStays);

            if (t.GetSiblingIndex() != siblingIndex)
                t.SetSiblingIndex(siblingIndex);

            if (index.HasValue)
                usedChildren.Insert(index.Value, t);
            else
                usedChildren.Add(t);

            unusedChildren.Remove(t);

            child.GetPrimer().parentContainer = this as Container<Component>;
        }

        public Transform Add(string name = null, bool worldPositionStays = false)
        {
            var child = FindChild<Transform>(name) ?? OnCreate(new GameObject(name).transform);
            Insert(child, worldPositionStays);
            return child;
        }

        public TChild Add<TChild>(string name = null, bool worldPositionStays = false) where TChild : Component
        {
            var child = FindChild<TChild>(name) ?? OnCreate(new GameObject(name).AddComponent<TChild>());
            Insert(child, worldPositionStays);
            return child;
        }

        public TChild Add<TChild>(TChild template, string name = null, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = FindInstanceOf(template, name)
                ?? OnCreate(InstantiationTracker.InstantiateAndRegister(template, name));

            Insert(child, worldPositionStays);
            return child;
        }

        public TChild Add<TChild>(PrefabProvider<TChild> provider, string name = null, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = FindInstanceOf(provider.value, name)
                ?? OnCreate(InstantiationTracker.InstantiateAndRegister(provider.value, name));

            Insert(child, worldPositionStays);
            provider.Initialize(child);
            return child;
        }

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
    }
}
