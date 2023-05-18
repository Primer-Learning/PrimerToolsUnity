using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public partial class Container<TComponent>
    {
        private readonly List<Transform> usedChildren = new();
        private readonly List<Transform> unusedChildren;
        private readonly List<Action> onPurge = new();
        private readonly List<Action> onCleanup = new();

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
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : new GameObject(name);
            return obj.transform;
        }

        private static T GetRootCloneOf<T>(T template, string name) where T : Component
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();

            var found = rootGameObjects
                .FirstOrDefault(x => x.name == name && InstantiationTracker.IsInstanceOf(x, template))
                ?.GetComponent<T>();

            return found != null ? found : InstantiationTracker.InstantiateAndRegister(template, name);
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
    }
}
