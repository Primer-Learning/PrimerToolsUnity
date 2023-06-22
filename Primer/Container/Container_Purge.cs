using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        private readonly List<Action> onCleanup = new();
        private readonly List<Action<bool>> onPurge = new();

        public void Purge(bool defer = false)
        {
            foreach (var child in unusedChildren)
                OnRemove(child, defer);

            foreach (var listener in onPurge)
                listener(defer);

            unusedChildren.Clear();
        }

        public void OnCleanup(Action action)
        {
            onCleanup.Add(action);
        }

        public void Dispose()
        {
            foreach (var broomUp in onCleanup)
                broomUp();
        }

        internal void RegisterChildContainer(Container container)
        {
            onPurge.Add(container.Purge);
        }
    }
}
