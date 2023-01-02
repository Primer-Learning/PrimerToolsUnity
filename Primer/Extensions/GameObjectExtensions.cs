using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer
{
    public static class GameObjectExtensions
    {
        public static async void Dispose(this GameObject gameObject, bool urgent = false)
        {
            if (!gameObject)
                return;


            if (!urgent)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

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
            => GetOrAddComponent<PrimerBehaviour>(gameObject);

        // Extension methods work on null values!
        public static bool IsNull(this GameObject gameObject) => gameObject == null;

        #region Visibility
        public static void Hide(this GameObject go)
        {
            var pos = go.transform.position;

            if (pos.z <= 0)
                return;

            pos.z *= -1;
            go.transform.position = pos;
        }

        public static void Show(this GameObject go)
        {
            var pos = go.transform.position;

            if (pos.z >= 0)
                return;

            pos.z *= -1f;
            go.transform.position = pos;
        }
        #endregion
    }
}
