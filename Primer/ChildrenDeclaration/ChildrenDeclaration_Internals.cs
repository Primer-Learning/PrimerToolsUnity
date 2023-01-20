using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class ChildrenDeclaration
    {
        private static void DefaultOnRemove(Transform x) => x.Dispose();


        private bool recreateNextChild;
        private readonly Transform parent;

        private readonly List<Transform> remaining = new();
        private readonly List<Transform> after = new();

        private readonly Action<Component> onCreate;
        private readonly Action<Transform> onRemove;

        private T UseCache<T>(T component) where T : Component
            => recreateNextChild || component == null ? null : Add(component);

        private void Initialize<T>(T transform, string name, Action<T> init = null) where T : Component
        {
            if (init is not null)
                init(transform);

            if (name is not null)
                transform.gameObject.name = name;

            if (onCreate is not null)
                onCreate(transform);
        }

        private T Add<T>(T target) where T : Component
        {
            NextIs(target.transform);
            recreateNextChild = false;
            return target;
        }


        private bool Find(out Transform found, string name, Component prefab = null)
        {
            if (recreateNextChild) {
                found = null;
                return false;
            }

            found = remaining.Find(
                child =>
                    (name is null || child.gameObject.name == name)
                    && (prefab is null || InstantiationTracker.IsInstanceOf(child, prefab))
            );

            return found is not null;
        }

        private bool Find<T>(out T found, string name, T prefab = null) where T : Component
        {
            if (!Find(out Transform foundTransform, name, prefab)) {
                found = null;
                return false;
            }

            found = foundTransform.GetComponent<T>();
            return found is not null;
        }
    }
}
