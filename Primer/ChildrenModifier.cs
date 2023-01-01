using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public class ChildrenModifier
    {
        private readonly List<Transform> after = new();
        private readonly List<Transform> before = new();
        private readonly Transform parent;

        public ChildrenModifier(Transform parent)
        {
            this.parent = parent;
            ReadChildren();
        }

        public int NextIndex { get; private set; }

        public Transform NextMustBeCalled(string name)
        {
            return GetOrCreate(NextIndex++, name);
        }

        public void NextMustBe(Transform transform)
        {
            after.Add(transform);
        }

        public void Apply()
        {
            var needsReorder = false;

            foreach (var remove in before.Where(x => !after.Contains(x)))
                remove.Dispose();

            foreach (var child in after) {
                var pos = child.localPosition;

                if (child.parent == parent) {
                    if (needsReorder)
                        child.parent = null;
                    else
                        continue;
                }

                child.parent = parent;
                child.localPosition = pos;
                needsReorder = true;
            }
        }

        private void Set(int index, Transform child)
        {
            if (after.Count <= index) {
                after.Insert(index, child);
            }
            else {
                after[index] = child;
            }
        }

        private Transform GetOrCreate(int index, string name)
        {
            var child = before.Find(x => x.gameObject.name == name)
                     ?? new GameObject { name = name }.transform;

            Set(index, child);
            return child;
        }

        private void ReadChildren()
        {
            var childCount = parent.childCount;

            before.Clear();

            for (var i = 0; i < childCount; i++)
                before.Add(parent.GetChild(i));
        }
    }
}
