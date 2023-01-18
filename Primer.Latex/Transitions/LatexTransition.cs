using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer.Latex
{
    internal class LatexTransition : IDisposable
    {
        private static int instances;

        private readonly GameObject container;
        private readonly AnimationCurve curve;

        private readonly LatexTransitionState start;
        private readonly LatexTransitionState end;
        private readonly Vector3 offset;

        private readonly List<(Transform, GroupState)> add = new();
        private readonly List<(Transform, GroupState)> remove = new();
        private readonly List<(Transform, GroupState from, GroupState to)> transition = new();


        public Transform transform => container.transform;


        public LatexTransition(LatexTransitionState from, LatexTransitionState to, AnimationCurve curve)
        {
            this.curve = curve;
            offset = from.GetOffsetWith(to);
            start = from;
            end = to;

            container = new GameObject($"LatexTransition {instances++}") {
                hideFlags = HideFlags.DontSave,
            };

            CreateTransforms();
        }


        public void Dispose() => container.Dispose(urgent: true);

        public bool Is(LatexTransitionState from, LatexTransitionState to) => (start == from) && (end == to);


        public void Apply(float t)
        {
            var eased = curve.Evaluate(t);
            var easeOut = 1 - Mathf.Clamp(t * 2, 0, 1);
            var easeIn = Mathf.Clamp(t * 2 - 1, 0, 1);

            foreach (var (groupTransform, group) in remove)
                groupTransform.localScale = group.scale * easeOut;

            foreach (var (groupTransform, group) in add)
                groupTransform.localScale = group.scale * easeIn;

            foreach (var (groupTransform, before, after) in transition) {
                groupTransform.localScale = Vector3.Lerp(before.scale, after.scale, eased);
                groupTransform.localPosition = Vector3.Lerp(before.position, after.position + offset, eased);
            }
        }

        private void CreateTransforms()
        {
            var parent = container.transform;

            foreach (var (before, after) in start.GetCommonGroups(end)) {
                var groupTransform = Object.Instantiate(before.transform, parent);
                groupTransform.localPosition += offset;
                transition.Add((groupTransform, before, after));
            }

            foreach (var group in start.GroupsToRemoveTransitioningTo(end)) {
                var groupTransform = Object.Instantiate(group.transform, parent);
                groupTransform.localScale = Vector3.zero;
                remove.Add((groupTransform, group));
            }

            foreach (var group in start.GroupsToAddTransitioningTo(end)) {
                var groupTransform = Object.Instantiate(group.transform, parent);
                groupTransform.localPosition += offset;
                groupTransform.localScale = Vector3.zero;
                add.Add((groupTransform, group));
            }
        }
    }
}
