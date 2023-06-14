using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class Container_AddPrefabExtensions
    {
        private static readonly Dictionary<string, Component> cachedPrefabs = new();

        public static T AddPrefab<T>(this Container container, string prefabName, string name = null)
            where T : Component
        {
            var prefab = GetPrefabByName<T>(prefabName);
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
