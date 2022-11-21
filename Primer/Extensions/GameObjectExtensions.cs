using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class GameObjectExtensions
    {
        // Extension methods work on null values!
        public static bool IsNull(this GameObject gameObject) => gameObject == null;

        public static void Dispose(this GameObject gameObject) {
            if (Application.isEditor) {
                Object.DestroyImmediate(gameObject);
            }
            else {
                Object.Destroy(gameObject);
            }
        }

        public static void DisposeAll(this IEnumerable<GameObject> list) {
            var array = list.ToArray();

            for (var i = array.Length - 1; i >= 0; i--) {
                Dispose(array[i]);
            }
        }

        public static void DisposeAll(this IEnumerable<Transform> list) {
            var array = list.ToArray();

            for (var i = array.Length - 1; i >= 0; i--) {
                if (array[i] != null) {
                    Dispose(array[i].gameObject);
                }
            }
        }
    }
}
