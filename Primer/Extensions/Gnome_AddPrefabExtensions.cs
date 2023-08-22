using UnityEngine;

namespace Primer
{
    // This is defined as an extension method because it doesn't need access to the internals of Gnome
    // But can become another _Partial class
    public static class Gnome_AddPrefabExtensions
    {
        public static T AddPrefab<T>(this Gnome gnome, string prefabName, string name = null,
            ChildOptions options = null)
            where T : Component
        {
            var prefab = Prefab.Get<T>(prefabName);

            if (prefab != null)
                return gnome.Add(prefab, name, options);

            var prefabTransform = Prefab.Get(prefabName);
            var child = gnome.Add(prefabTransform, name, options);
            return child.GetOrAddComponent<T>();
        }
    }
}
