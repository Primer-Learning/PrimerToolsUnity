using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    public interface IChildDefinition
    {
        public IChildDefinition Called(string name);

        public IChildDefinition Initialize(Action<Transform> initializer);

        public Transform Get();

        public T WithComponent<T>() where T : Component;

        public T InstantiatedFrom<T>(T prefab) where T : Component;


        /// <summary>
        ///     Provides a serializable way of tracking which prefab was used to instantiate an object.
        ///     This is an internal tool and shouldn't be used manually.
        /// </summary>
        [DisallowMultipleComponent]
        internal class InstantiationTracker : MonoBehaviour
        {
            public static bool IsInstanceOf(Component target, Component prefab)
            {
                var tracker = target.GetComponent<InstantiationTracker>();
                return tracker is not null && (tracker.from == prefab.gameObject.name);
            }

            public static Component Instantiate(Component prefab, Transform parent, bool worldPositionStays = false)
            {
                var target = Object.Instantiate(prefab, parent, worldPositionStays);
                var tracker = target.gameObject.AddComponent<InstantiationTracker>();
                tracker.hideFlags = HideFlags.HideInInspector;
                tracker.from = prefab.gameObject.name;
                return target;
            }

            public string from;
        }
    }
}
