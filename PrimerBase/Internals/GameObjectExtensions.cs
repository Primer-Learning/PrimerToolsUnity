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
    }
}
