using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public class ChildrenModifier
    {
        private static void DefaultOnRemove(Transform x) => x.Dispose();


        internal readonly Transform parent;

        private readonly ChildFinderOrCreator definition = new();
        private readonly ChildValidator validator = new();

        private readonly List<Transform> after = new();
        internal readonly List<Transform> remaining = new();

        public Action<Transform> onCreate = null;
        public Action<Transform> onRemove = DefaultOnRemove;


        public ChildrenModifier(Transform parent)
        {
            this.parent = parent;

            definition.owner = this;
            validator.owner = this;

            ReadChildren();
        }


        public IChildDefinition Next()
        {
            definition.Clear();
            definition.init.Add(onCreate);
            return definition;
        }

        public IChildDefinition Next(string name) => Next().Called(name);

        public Transform NextMustBeCalled(string name) => Next().Called(name).Get();

        public IChildDefinition Next(Component child)
        {
            if (child == null)
                return Next();

            validator.Reset(child);
            return validator;
        }

        public void NextMustBe(Component child)
        {
            if (!child)
                return;

            var transform = child.transform;
            after.Add(transform);
            remaining.Remove(transform);
        }


        public void Apply()
        {
            var needsReorder = false;

            foreach (var remove in remaining)
                onRemove(remove);

            foreach (var child in after) {
                var pos = child.localPosition;
                var scale = child.localScale;

                if (child.parent == parent) {
                    if (needsReorder)
                        child.parent = null;
                    else
                        continue;
                }

                child.parent = parent;
                child.localPosition = pos;
                child.localScale = scale;
                needsReorder = true;
            }
        }

        private void ReadChildren()
        {
            var childCount = parent.childCount;

            remaining.Clear();

            for (var i = 0; i < childCount; i++) {
                var child = parent.GetChild(i);
                remaining.Add(child);
            }
        }
    }
}
