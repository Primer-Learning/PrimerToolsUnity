using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer.Latex
{
    internal class LatexTransition : IDisposable
    {
        private static int instances;

        private readonly GameObject gameObject;
        private readonly AnimationCurve curve;
        private readonly TransformSnapshot endStateSnapshot;

        private readonly LatexTransitionState start;
        private readonly LatexTransitionState end;
        private readonly Vector3 offset;

        private readonly List<(Transform, GroupState)> add = new();
        private readonly List<(Transform, GroupState)> remove = new();
        private readonly List<(Transform, GroupState from, GroupState to)> transition = new();

        private enum State { Ready, Transitioning, Ended }
        private State state;

        public Transform transform => gameObject.transform;


        public LatexTransition(LatexTransitionState from, LatexTransitionState to, AnimationCurve curve)
        {
            this.curve = curve;
            offset = from.GetOffsetWith(to);
            start = from;
            end = to;
            endStateSnapshot = new TransformSnapshot(end.transform);

            gameObject = CreateTransitionGameObject();
            state = State.Ready;
        }

        public void Dispose()
        {
            gameObject.Dispose(urgent: false);
            endStateSnapshot?.Restore();
            end.gameObject.SetActive(false);
            start.gameObject.SetActive(true);
        }

        public bool Is(LatexTransitionState from, LatexTransitionState to)
        {
            return start == from && end == to;
        }


        private void StartTransition()
        {
            state = State.Transitioning;
            start.gameObject.SetActive(false);
            gameObject.SetActive(true);
            end.gameObject.SetActive(false);
        }

        private void EndTransition()
        {
            state = State.Ended;
            start.transform.CopyTo(end.transform);
            start.gameObject.SetActive(false);
            gameObject.SetActive(false);
            end.gameObject.SetActive(true);
        }

        public void Apply(float t)
        {
            if (t >= 1) {
                if (state is not State.Ended)
                    EndTransition();

                return;
            }

            if (state is State.Ready or State.Ended)
                StartTransition();

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

        private GameObject CreateTransitionGameObject()
        {
            var go = new GameObject($"LatexTransition {instances++}") {
                hideFlags = HideFlags.DontSave,
            };

            go.SetActive(false);
            start.transform.CopyTo(go.transform);
            CreateGroups(go.transform);
            return go;
        }

        private void CreateGroups(Transform parent)
        {
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
