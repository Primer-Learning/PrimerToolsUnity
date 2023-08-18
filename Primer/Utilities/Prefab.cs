using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class Prefab
    {
        private static readonly Dictionary<string, Component> cachedPrefabs = new();

        // This didn't work as expected
        // As a result we may not be unloading the assets properly :(
        //
        // private static readonly Cleaner cleaner = new Cleaner();

        public static Transform Get(string prefabName)
            => Get<Transform>(prefabName);

        public static T Get<T>(string prefabName) where T : Component
        {
            if (cachedPrefabs.TryGetValue(prefabName, out var cachedPrefab)) {
                if (cachedPrefab != null)
                    return cachedPrefab as T;

                cachedPrefabs.Remove(prefabName);
            }

            var prefab = Resources.Load<T>(prefabName);
            cachedPrefabs.Add(prefabName, prefab);
            return prefab;
        }

        // private sealed class Cleaner
        // {
        //     ~Cleaner()
        //     {
        //         foreach (var prefab in cachedPrefabs.Values) {
        //             Resources.UnloadAsset(prefab);
        //         }
        //     }
        // }
    }
}
