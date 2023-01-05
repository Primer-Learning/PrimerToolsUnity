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

        private readonly LatexTransitionState end;
        private readonly LatexTransitionState start;
        private readonly Dictionary<GroupState, Transform> transforms;

        public LatexTransition(LatexTransitionState from, LatexTransitionState to, AnimationCurve curve)
        {
            this.curve = curve;

            start = from;
            end = to;

            container = new GameObject($"LatexTransition {instances++}") {
                hideFlags = HideFlags.DontSave,
            };

            transforms = CreateTransforms();
        }

        public void Dispose() => container.Dispose(urgent: true);

        public bool Is(LatexTransitionState from, LatexTransitionState to) => (start == from) && (end == to);

        public void Place(Transform parent, Vector3 position)
        {
            container.transform.parent = parent;
            container.transform.position = position;
        }

        public void Apply(float t)
        {
            var eased = curve.Evaluate(t);
            var easeOut = 1 - Mathf.Clamp(t * 2, 0, 1);
            var easeIn = Mathf.Clamp(t * 2 - 1, 0, 1);

            foreach (var group in start.GroupsToRemoveTransitioningTo(end)) {
                var transform = transforms[group];
                transform.localScale = group.scale * easeOut;
            }

            foreach (var group in end.GroupsToRemoveTransitioningTo(start)) {
                var transform = transforms[group];
                transform.localScale = group.scale * easeIn;
            }

            foreach (var (before, after) in start.GetCommonGroups(end)) {
                var transform = transforms[before];
                transform.localScale = Vector3.Lerp(before.scale, after.scale, eased);
                transform.localPosition = Vector3.Lerp(before.position, after.position, eased);
            }
        }

        private Dictionary<GroupState, Transform> CreateTransforms()
        {
            var parent = container.transform;
            var result = new Dictionary<GroupState, Transform>();

            foreach (var (before, after) in start.GetCommonGroups(end)) {
                var transform = Object.Instantiate(before.transform, parent);
                result.Add(before, transform);
                result.Add(after, transform);
            }

            foreach (var group in start.GroupsToRemoveTransitioningTo(end)) {
                var transform = Object.Instantiate(group.transform, parent);
                transform.localScale = Vector3.zero;
                result.Add(group, transform);
            }

            foreach (var group in end.GroupsToRemoveTransitioningTo(start)) {
                var transform = Object.Instantiate(group.transform, parent);
                transform.localScale = Vector3.zero;
                result.Add(group, transform);
            }

            return result;
        }
    }
}
