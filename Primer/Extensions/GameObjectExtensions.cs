using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class GameObjectExtensions
    {
        public static void Dispose(this GameObject gameObject)
        {
            if (gameObject == null)
                return;

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                Object.DestroyImmediate(gameObject);
            else
#endif
                Object.Destroy(gameObject);
        }

        public static void DisposeAll(this IEnumerable<Transform> list)
        {
            var array = list.ToArray();

            for (var i = array.Length - 1; i >= 0; i--) {
                if (array[i] != null) {
                    Dispose(array[i].gameObject);
                }
            }
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var found = gameObject.GetComponent<T>();
            return found == null ? gameObject.AddComponent<T>() : found;
        }

        public static PrimerBehaviour GetPrimer(this GameObject gameObject)
        {
            return GetOrAddComponent<PrimerBehaviour>(gameObject);
        }

        // Extension methods work on null values!
        public static bool IsNull(this GameObject gameObject)
        {
            return gameObject == null;
        }
    }
}
