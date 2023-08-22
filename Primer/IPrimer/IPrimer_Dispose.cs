using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_DisposeExtensions
    {
        public static void Dispose(this IEnumerable<IPrimer> self, bool defer = false)
        {
            foreach (var item in self)
                item.Dispose(defer);
        }

        public static void Dispose(this IEnumerable<Component> self, bool defer = false)
        {
            foreach (var item in self)
                item.Dispose(defer);
        }

        public static void Dispose(this IPrimer self) => self.Dispose(false);
        public static void Dispose(this IPrimer self, bool defer)
        {
            if (self != null)
                self.transform.gameObject.Dispose(defer);
        }

        public static void Dispose(this Component self) => self.Dispose(false);
        public static void Dispose(this Component self, bool defer)
        {
            if (self != null)
                self.gameObject.Dispose(defer);
        }

        // Actual implementation
        // All overloads redirect here
        public static async void Dispose(this GameObject gameObject, bool defer = false)
        {
            if (!gameObject)
                return;

            if (defer)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                Object.DestroyImmediate(gameObject);
            else
#endif
                Object.Destroy(gameObject);
        }

        // DisposeComponent is a special case
        public static async void DisposeComponent(this Component self, bool defer = false)
        {
            if (!self)
                return;

            // TODO: invert this boolean, the boolean should be called `delayed` and only run this line if true
            if (defer)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                Object.DestroyImmediate(self);
            else
#endif
                Object.Destroy(self);
        }
    }
}
