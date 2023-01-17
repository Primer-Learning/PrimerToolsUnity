using UnityEngine;

namespace Primer
{
    public static class ComponentExtensions
    {
        public static void Dispose(this Component component)
        {
            if (component != null)
                component.gameObject.Dispose();
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            var exists = component.GetComponent<T>();

            return exists != null
                ? exists
                : component.gameObject.AddComponent<T>();
        }

        public static PrimerBehaviour GetPrimer(this Component component)
        {
            return GetOrAddComponent<PrimerBehaviour>(component);
        }

#if UNITY_EDITOR
        public static void Log(this Component component, params object[] data)
        {
            var type = component.GetType().Name;
            var gameObject = component.gameObject;
            var name = gameObject.name;
            var isPrefab = gameObject.IsPreset() ? " (prefab)" : "";

            var parent = component.transform.parent;
            var parentName = parent == null ? "" : $"{parent.gameObject.name} > ";

            var parts = new string[data.Length];
            for (var i = 0; i < data.Length; i++)
                parts[i] = data[i] == null ? "null" : data[i].ToString();

            Debug.Log($"{type}{isPrefab} [{parentName}{name}] {string.Join(" - ", parts)}\n\n");
        }
#endif
    }
}
