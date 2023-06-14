using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        private readonly List<Action> onCleanup = new();

        public void Purge()
        {
            foreach (var child in unusedChildren)
                OnRemove(child);

            foreach (var listener in onPurge)
                listener();

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
    }
}
