using UnityEngine;

namespace Primer
{
    public static class Container_AddPrefabExtensions
    {
        public static T AddPrefab<T>(this Container container, string prefabName, string name = null,
            ChildOptions options = null)
            where T : Component
        {
            var prefab = Prefab.Get<T>(prefabName);
            return container.Add(prefab, name, options);
        }
    }
}
