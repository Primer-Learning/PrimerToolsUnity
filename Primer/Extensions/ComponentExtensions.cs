using System;
using UnityEngine;

namespace Primer
{
    public static class ComponentExtensions
    {
        public static void Dispose(this Component component, bool urgent = false)
        {
            if (component != null)
                component.gameObject.Dispose(urgent);
        }

        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            var exists = component.GetComponent<T>();

            return exists != null
                ? exists
                : component.gameObject.AddComponent<T>();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public static PrimerBehaviour GetPrimer(this Component component)
        {
            return GetOrAddComponent<PrimerBehaviour>(component);
        }

#if UNITY_EDITOR
        public static void Log(this Component component, params object[] data)
            => PrimerLogger.Log(component, data);

        public static void Error(this Component component, Exception exception)
            => PrimerLogger.Error(component, exception);
#endif
    }
}
