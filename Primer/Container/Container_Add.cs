using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
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
    }
}
