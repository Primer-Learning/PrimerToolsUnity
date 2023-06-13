using UnityEngine;

namespace Primer
{
    public static class IPrimer_HasComponentExtensions
    {
        public static bool HasComponent<T>(this IPrimer self) where T : Component
        {
            return self.component.gameObject.HasComponent<T>();
        }

        public static bool HasComponent<T>(this Component self) where T : Component
        {
            return self.gameObject.HasComponent<T>();
        }

        // Actual implementation
        // All overloads redirect here
        //
        // ReSharper disable Unity.PerformanceAnalysis
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            var found = gameObject.GetComponent<T>();
            return found != null;
        }
    }
}
