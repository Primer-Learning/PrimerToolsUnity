using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer
{
    public static class IPrimer_DisposeExtensions
    {
        public static void Dispose(this IEnumerable<IPrimer> self, bool urgent = false)
        {
            foreach (var item in self)
                item.transform.gameObject.Dispose(urgent);
        }

        public static void Dispose(this IEnumerable<Component> self, bool urgent = false)
        {
            foreach (var item in self)
                item.gameObject.Dispose(urgent);
        }

        public static void Dispose(this IPrimer self, bool urgent = false)
        {
            if (self != null)
                self.transform.gameObject.Dispose(urgent);
        }

        public static void Dispose(this Component self, bool urgent = false)
        {
            if (self != null)
                self.gameObject.Dispose(urgent);
        }

        // Actual implementation
        // All overloads redirect here
        public static async void Dispose(this GameObject gameObject, bool urgent = false)
        {
            if (!gameObject)
                return;

            // TODO: invert this boolean, the boolean should be called `delayed` and only run this line if true
            if (!urgent)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                Object.DestroyImmediate(gameObject);
            else
#endif
                Object.Destroy(gameObject);
        }

    }
}
