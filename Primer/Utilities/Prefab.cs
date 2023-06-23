using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class Prefab
    {
        private static readonly Dictionary<string, Component> cachedPrefabs = new();
        private static readonly Cleaner cleaner = new Cleaner();

        public static GameObject Get(string prefabName)
            => Get<Transform>(prefabName).gameObject;

        public static T Get<T>(string prefabName) where T : Component
        {
            if (cachedPrefabs.TryGetValue(prefabName, out var cachedPrefab)) {
                return cachedPrefab as T;
            }

            var prefab = Resources.Load<T>(prefabName);
            cachedPrefabs.Add(prefabName, prefab);
            return prefab;
        }

        private sealed class Cleaner
        {
            ~Cleaner()
            {
                foreach (var prefab in cachedPrefabs.Values) {
                    Resources.UnloadAsset(prefab);
                }
            }
        }
    }
}
