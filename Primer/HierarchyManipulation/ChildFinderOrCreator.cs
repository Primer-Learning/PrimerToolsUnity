using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    internal class ChildFinderOrCreator : IChildDefinition
    {
        internal ChildrenModifier owner;

        private static readonly Func<Transform> createEmpty = () => new GameObject().transform;
        internal Func<Transform> create;
        internal readonly List<Predicate<Transform>> conditions = new();
        internal readonly List<Action<Transform>> init = new();


        public IChildDefinition Called(string name)
        {
            conditions.Add(x => x.gameObject.name == name);
            init.Add(x => x.gameObject.name = name);
            return this;
        }

        public IChildDefinition Initialize(Action<Transform> initializer)
        {
            init.Add(initializer);
            return this;
        }


        public T WithComponent<T>() where T : Component
        {
            conditions.Add(x => x.GetComponent<T>() is not null);
            init.Add(x => x.gameObject.AddComponent<T>());
            return Get<T>();
        }

        public T InstantiatedFrom<T>(T prefab) where T : Component
        {
            create = () => IChildDefinition.InstantiationTracker.Instantiate(prefab, owner.parent).transform;
            conditions.Add(x => IChildDefinition.InstantiationTracker.IsInstanceOf(x, prefab));
            return Get<T>();
        }


        public Transform Get()
        {
            var child = Execute();
            Clear();
            owner.NextMustBe(child);
            return child;
        }


        private T Get<T>() where T : Component
        {
            var child = Get();
            return child.GetComponent<T>();
        }

        internal void Clear()
        {
            create = null;
            init.Clear();
            conditions.Clear();
        }

        private Transform Execute()
        {
            var found = owner.remaining.Find(item => conditions.All(predicate => predicate(item)));

            if (found is not null)
                return found;

            var child = (create ?? createEmpty)();
            var go = child.gameObject;

            for (var i = 0; i < init.Count; i++) {
                init[i](child);

                // This can happen if the Transform is replaced with a RectTransform
                if (child == null)
                    child = go.transform;
            }

            return child;
        }
    }
}
