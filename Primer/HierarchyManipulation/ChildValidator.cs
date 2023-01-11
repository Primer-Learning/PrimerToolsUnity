using System;
using UnityEngine;

namespace Primer
{
    internal class ChildValidator : IChildDefinition
    {
        internal ChildrenModifier owner;
        private Component target;

        internal void Reset(Component newTarget)
        {
            target = newTarget;
        }

        public IChildDefinition Called(string name)
        {
            if (target.gameObject.name != name)
                throw new ChildValidationException($"Child name is not {name}");

            return this;
        }

        public IChildDefinition Initialize(Action<Transform> initializer) => this;

        public Transform Get()
        {
            owner.NextMustBe(target);
            return target.transform;
        }

        public T WithComponent<T>() where T : Component
        {
            var component = target as T ?? target.GetComponent<T>();

            if (component == null) {
                throw new ChildValidationException(
                    $"Child is not of type {typeof(T).FullName}. {target.GetType().FullName} found"
                );
            }

            owner.NextMustBe(target);
            return component;
        }

        public T InstantiatedFrom<T>(T prefab) where T : Component
        {
            if (!IChildDefinition.InstantiationTracker.IsInstanceOf(target, prefab))
                throw new ChildValidationException($"Child is not instance of {prefab.name}.");

            return WithComponent<T>();
        }
    }
}
