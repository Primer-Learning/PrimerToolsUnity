using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    // This part handles events like child creation and removal
    public partial class Gnome
    {
        private static bool areEventsDeactivated
            => GnomeEvents.deactivateEventsIf?.Invoke() ?? Application.isPlaying;

        public Action<Transform> onCreate;
        public Action<Transform, bool> onRemove;

        private readonly List<Transform> created = new();
        public IEnumerable<Transform> removing => GetChildrenBeingRemoved(transform);

        public bool IsCreated(Component child)
        {
            return created.Contains(child.transform);
        }

        public IReadOnlyList<Transform> GetCreatedChildren()
        {
            return created;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private TChild OnCreate<TChild>(TChild child) where TChild : Component
        {
            created.Add(child.transform);

            if (onCreate is null || areEventsDeactivated)
                return child;

            onCreate.Invoke(child.transform);
            return child;
        }

        private void OnRemove(Component child, bool defer)
        {
            if (onRemove is null || areEventsDeactivated) {
                child.Dispose(defer);
                return;
            }

            child.GetOrAddComponent<GnomeEvents.IsRemoving>();
            onRemove.Invoke(child.transform, defer);
        }

        private static List<Transform> ReadExistingChildren(Transform transform)
        {
            return transform.GetChildren()
                .Where(x => !x.HasComponent<GnomeEvents.IsRemoving>())
                .ToList();
        }

        private static IEnumerable<Transform> GetChildrenBeingRemoved(Transform transform)
        {
            return transform.GetChildren()
                .Where(x => x.HasComponent<GnomeEvents.IsRemoving>());
        }
    }

    // We need a non-generic class to contain the static values
    internal static class GnomeEvents
    {
        internal static Action<Transform, bool> defaultOnRemove = (x, defer) => x.Dispose(defer);

        /// <summary>
        ///   If this function returns true, the gnome will not fire any events.
        /// </summary>
        internal static Func<bool> deactivateEventsIf = null;

        /// <summary>
        ///   This component is added to a GameObject when it is being removed from the gnome.
        /// </summary>
        [DisallowMultipleComponent]
        internal class IsRemoving : MonoBehaviour {}
    }
}
