using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
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

        public Container<TChild> AddContainer<TChild>(PrefabProvider<TChild> provider, string name, bool worldPositionStays = false)
            where TChild : Component
        {
            var child = Add(provider, name, worldPositionStays);
            var childContainer = new Container<TChild>(child);
            onPurge.Add(() => childContainer.Purge());
            return childContainer;
        }
    }
}
