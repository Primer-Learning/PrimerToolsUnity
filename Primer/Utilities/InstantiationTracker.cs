using UnityEngine;

namespace Primer
{
    [DisallowMultipleComponent]
    internal class InstantiationTracker : MonoBehaviour
    {
        public static bool IsInstanceOf(GameObject target, Component prefab)
        {
            return IsInstanceOf(target.transform, prefab);
        }

        public static bool IsInstanceOf(Component target, Component prefab)
        {
            var tracker = target.GetComponent<InstantiationTracker>();
            return tracker is not null && tracker.template == prefab.transform;
        }

        public static T InstantiateAndRegister<T>(T prefab, string name)
            where T : Component
        {
            var target = Instantiate(prefab);

            if (!string.IsNullOrWhiteSpace(name))
                target.name = name;

            var tracker = target.GetComponent<InstantiationTracker>()
                ?? target.gameObject.AddComponent<InstantiationTracker>();

            tracker.hideFlags = HideFlags.HideInInspector;
            tracker.template = prefab.transform;
            return target;
        }

        public Transform template;
    }
}
