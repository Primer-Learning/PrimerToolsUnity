using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        private readonly List<Transform> usedChildren = new();
        private readonly List<Transform> unusedChildren = new();

        public int childCount => usedChildren.Count;

        public void Insert<TChild>(TChild child, bool worldPositionStays = false, int? index = null)
            where TChild : Component
        {
            var siblingIndex = index ?? childCount;
            var t = child.transform;

            t.SetActive(true);

            if (t.parent != transform)
                t.SetParent(transform, worldPositionStays);

            if (t.GetSiblingIndex() != siblingIndex)
                t.SetSiblingIndex(siblingIndex);

            if (index.HasValue)
                usedChildren.Insert(index.Value, t);
            else
                usedChildren.Add(t);

            unusedChildren.Remove(t);

            child.GetPrimer().parentContainer = this as Container<Component>;
        }
    }
}
