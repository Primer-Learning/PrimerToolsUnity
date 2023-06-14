using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        private readonly List<Action> onPurge = new();

        public Container<TChild> WrapChild<TChild>(TChild child)
            where TChild : Component
        {
            if (!usedChildren.Contains(child.transform))
                throw new ArgumentException($"Child {child.name} is not a child of this container", nameof(child));

            return CreateChildContainerFor(child);
        }

        public Container AddContainer(string name, bool worldPositionStays = false)
        {
            var child = Add(name, worldPositionStays);
            return CreateChildContainerFor(child);
        }

        public Container<TChild> AddContainer<TChild>(string name, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = Add<TChild>(name, worldPositionStays);
            return CreateChildContainerFor(child);
        }

        public Container<TChild> AddContainer<TChild>(TChild template, string name, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = Add(template, name, worldPositionStays);
            return CreateChildContainerFor(child);
        }

        public Container<TChild> AddContainer<TChild>(PrefabProvider<TChild> provider, string name,
            bool worldPositionStays = false)
            where TChild : Component
        {
            var child = Add(provider, name, worldPositionStays);
            return CreateChildContainerFor(child);
        }

        private Container<TChild> CreateChildContainerFor<TChild>(TChild child) where TChild : Component
        {
            var childContainer = new Container<TChild>(child);
            onPurge.Add(childContainer.Purge);
            return childContainer;
        }
    }
}
