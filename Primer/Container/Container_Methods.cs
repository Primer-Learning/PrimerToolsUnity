using System;
using UnityEngine;

namespace Primer
{
    public partial class Container<TComponent>
    {
        public void Insert<TChild>(TChild child, bool worldPositionStays = false, int? index = null)
            where TChild : Component
        {
            var position = index ?? childCount;
            var t = child.transform;

            t.SetActive(true);

            if (t.parent != transform)
                t.SetParent(transform, worldPositionStays);

            if (t.GetSiblingIndex() != position)
                t.SetSiblingIndex(position);

            if (index.HasValue)
                usedChildren.Insert(index.Value, t);
            else
                usedChildren.Add(t);

            unusedChildren.Remove(t);
        }

        public void Purge()
        {
            foreach (var child in unusedChildren)
                OnRemove(child);

            foreach (var listener in onPurge)
                listener();

            unusedChildren.Clear();
        }

        public void Reset()
        {
            usedChildren.Clear();
            unusedChildren.Clear();
            unusedChildren.AddRange(transform.GetChildren());
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
