using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class Container_AddPrefabExtensions
    {
        private static readonly Dictionary<string, Component> cachedPrefabs = new();

        public static Transform AddPrefab<T>(this Container<T> container, string prefabName, string name = null)
            where T : Component
        {
            return container.AddPrefab<T, Transform>(prefabName, name);
        }

        public static T AddPrefab<T>(this Container container, string prefabName, string name = null)
            where T : Component
        {
            return container.AddPrefab<Transform, T>(prefabName, name);
        }

        public static TResult AddPrefab<TContainer, TResult>(this Container<TContainer> container, string prefabName,
            string name = null)
            where TContainer : Component
            where TResult : Component
        {
            var prefab = GetPrefabByName<TResult>(prefabName);
            return container.Add(prefab, name);
        }

        private static T GetPrefabByName<T>(string prefabName) where T : Component
        {
            if (cachedPrefabs.ContainsKey(prefabName)) {
                return cachedPrefabs[prefabName] as T;
            }

            var prefab = Resources.Load<T>(prefabName);
            cachedPrefabs.Add(prefabName, prefab);
            return prefab;
        }
    }
}
