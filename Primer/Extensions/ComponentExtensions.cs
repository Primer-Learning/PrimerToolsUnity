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
    }
}
