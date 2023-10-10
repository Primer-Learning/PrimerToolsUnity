using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    // This part defines the .Insert() method. It has it's own partial class because EVERY children passes through it.
    public partial class Gnome
    {
        private readonly List<Transform> usedChildren = new();
        private readonly List<Transform> unusedChildren = new();

        public int childCount => usedChildren.Count;

        public void Insert<TChild>(TChild child, ChildOptions options = null)
            where TChild : Component
        {
            options ??= new ChildOptions();
            var t = child.transform;

            if (t.parent != transform)
                t.SetParent(transform, options.worldPositionStays);

            if (options.enable)
                t.SetActive(true);

            if (options.zeroScale)
                t.localScale = Vector3.zero;

            if (options.ignoreSiblingOrder) {
                usedChildren.Add(t);
            } else {
                var siblingIndex = (int?)options.siblingIndex ?? childCount;

                usedChildren.Insert(siblingIndex, t);

                if (t.GetSiblingIndex() != siblingIndex)
                    t.SetSiblingIndex(siblingIndex);
            }

            unusedChildren.Remove(t);

            child.GetPrimer().parentGnome = this;
        }
    }
}
