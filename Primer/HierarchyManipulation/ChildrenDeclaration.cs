using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    /// <summary>
    /// </summary>
    public class ChildrenDeclaration : IChildrenDeclaration
    {
        public static void Clear(Transform parent, Action<Transform> onRemove = null)
        {
            var remove = onRemove ?? DefaultOnRemove;

            for (int i = 0, childCount = parent.childCount; i < childCount; i++)
                remove(parent.GetChild(i));
        }

        private readonly Transform parent;

        private readonly List<Transform> remaining = new();
        private readonly List<Transform> after = new();

        private readonly Action<Component> onCreate;
        private readonly Action<Transform> onRemove;

        public int count => after.Count;


        public ChildrenDeclaration(
            Transform parent,
            Action<Component> onCreate = null,
            Action<Transform> onRemove = null)
        {
            DontCallMeOnPrefabException.ThrowIfIsPrefab(parent, nameof(ChildrenDeclaration));

            this.parent = parent;
            this.onCreate = onCreate;
            this.onRemove = onRemove ?? DefaultOnRemove;

            Reset();
        }

        public void Reset()
        {
            after.Clear();
            remaining.Clear();

            for (int i = 0, childCount = parent.childCount; i < childCount; i++) {
                var child = parent.GetChild(i);

                if (child != null)
                    remaining.Add(child);
            }
        }


        public void NextIs(Component target) => NextIs(target.transform);

        public void NextIs(Transform target)
        {
            after.Add(target);
            remaining.Remove(target);
        }


        #region Ways to declare children
        public Transform Next(ref Transform cache, string name = null, Action<Transform> init = null)
            => UseCache(cache) ?? (cache = Next(name, init));

        public Transform Next(string name = null, Action<Transform> init = null)
        {
            if (Find(out var found, name))
                return Add(found);

            var transform = new GameObject().transform;
            Initialize(transform, name, init);
            return Add(transform);
        }

        public T Next<T>(ref T cache, string name = null, Action<T> init = null) where T : Component
            => UseCache(cache) ?? (cache = Next(name, init));

        public T Next<T>(string name = null, Action<T> init = null) where T : Component
        {
            if (Find<T>(out var found, name))
                return Add(found);

            var component = new GameObject().AddComponent<T>();
            Initialize(component, name, init);
            return Add(component);
        }

        public T NextIsInstanceOf<T>(T prefab, ref T cache, string name = null, Action<T> init = null)
            where T : Component
            => UseCache(cache) ?? (cache = NextIsInstanceOf(prefab, name, init));

        public T NextIsInstanceOf<T>(T prefab, string name = null, Action<T> init = null) where T : Component
        {
            if (Find(out var found, name, prefab))
                return Add(found);

            var instance = InstantiationTracker.InstantiateAndRegister(prefab, parent);
            Initialize(instance, name, init);
            return Add(instance);
        }

        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            ref TCached cache,
            Func<TPrefab, TCached> init,
            string name = null)
            where TPrefab : Component
            where TCached : Component
            => UseCache(cache) ?? (cache = NextIsInstanceOf(prefab, init, name));

        public TCached NextIsInstanceOf<TPrefab, TCached>(
            TPrefab prefab,
            Func<TPrefab, TCached> init,
            string name = null)
            where TPrefab : Component
            where TCached : Component
        {
            if (Find(out Transform foundTransform, name, prefab)) {
                var found = foundTransform.GetComponent<TCached>();

                if (found is not null)
                    return Add(found);
            }

            var instance = InstantiationTracker.InstantiateAndRegister(prefab, parent);
            Initialize(instance, name);

            var result = init(instance);
            return Add(result);
        }
        #endregion


        #region Apply()
        public void Apply()
        {
            var needsReorder = false;

            foreach (var remove in remaining) {
                onRemove(remove);

                if (remove != null)
                    remove.parent = null;
            }

            foreach (var child in after) {
                var pos = child.localPosition;
                var scale = child.localScale;

                if (child.parent == parent) {
                    if (needsReorder)
                        child.parent = null;
                    else
                        continue;
                }

                child.parent = parent;
                child.localPosition = pos;
                child.localScale = scale;
                needsReorder = true;
            }
        }
        #endregion


        #region Find from existing children
        private bool Find(out Transform found, string name, Component prefab = null)
        {
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
        #endregion


        #region Internal
        private static void DefaultOnRemove(Transform x) => x.Dispose();

        private T UseCache<T>(T component) where T : Component
            => component == null ? null : Add(component);

        private void Initialize<T>(T transform, string name, Action<T> init = null) where T : Component
        {
            if (init is not null)
                init(transform);

            if (name is not null)
                transform.gameObject.name = name;

            if (onCreate is not null)
                onCreate(transform);
        }

        public T Add<T>(T target) where T : Component
        {
            NextIs(target.transform);
            return target;
        }
        #endregion


        #region class InstantiationTracker : MonoBehaviour
        [DisallowMultipleComponent]
        internal class InstantiationTracker : MonoBehaviour
        {
            public static bool IsInstanceOf(Component target, Component prefab)
            {
                var tracker = target.GetComponent<InstantiationTracker>();
                return tracker is not null && (tracker.from == prefab.gameObject.name);
            }

            public static T InstantiateAndRegister<T>(T prefab, Transform parent, bool worldPositionStays = false)
                where T : Component
            {
                var target = Instantiate(prefab, parent, worldPositionStays);
                var tracker = target.gameObject.AddComponent<InstantiationTracker>();
                tracker.hideFlags = HideFlags.HideInInspector;
                tracker.from = prefab.gameObject.name;
                return target;
            }

            public string from;
        }
        #endregion
    }
}
