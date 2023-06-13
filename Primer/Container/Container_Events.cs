using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    // We need a non-generic class to contain the static values
    internal static class ContainerEvents
    {
        internal static Action<Transform> defaultOnRemove = x => x.Dispose();

        /// <summary>
        ///   If this function returns true, the container will not fire any events.
        /// </summary>
        internal static Func<bool> deactivateEventsIf = null;

        /// <summary>
        ///   This component is added to a GameObject when it is being removed from the container.
        /// </summary>
        [DisallowMultipleComponent]
        internal class IsRemoving : MonoBehaviour {}
    }

    public partial class Container<TComponent>
    {
        private static bool areEventsDeactivated => ContainerEvents.deactivateEventsIf?.Invoke() ?? false;

        public Action<Transform> onCreate;
        public Action<Transform> onRemove;

        private TChild OnCreate<TChild>(TChild child) where TChild : Component
        {
            if (onCreate is null || areEventsDeactivated)
                return child;

            onCreate.Invoke(child.transform);
            return child;
        }

        private void OnRemove(Component child)
        {
            if (onRemove is null ||areEventsDeactivated) {
                ContainerEvents.defaultOnRemove.Invoke(child.transform);
                return;
            }

            child.GetOrAddComponent<ContainerEvents.IsRemoving>();
            onRemove.Invoke(child.transform);
        }

        private static List<Transform> ReadExistingChildren(Transform transform)
        {
            return transform.GetChildren()
                .Where(x => !x.HasComponent<ContainerEvents.IsRemoving>())
                .ToList();
        }

        private static IEnumerable<Transform> GetChildrenBeingRemoved(Transform transform)
        {
            return transform.GetChildren()
                .Where(x => x.HasComponent<ContainerEvents.IsRemoving>());
        }
    }
}
