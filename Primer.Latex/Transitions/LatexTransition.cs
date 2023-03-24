using System;
using System.Collections.Generic;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer.Latex
{
    public class LatexTransition : IDisposable
    {
        private static int instances;

        private readonly GameObject gameObject;
        private readonly EaseMode ease;
        private readonly Vector3 offset;

        private readonly LatexComponent start;
        private readonly LatexComponent end;
        private readonly TransitionType[] transitions;

        private readonly List<(Transform, Transform)> add = new();
        private readonly List<(Transform, Transform)> remove = new();
        private readonly List<(Transform, Transform from, Transform to)> transition = new();

        public Transform transform => gameObject.transform;


        public LatexTransition(LatexComponent from, LatexComponent to, IEnumerable<TransitionType> transitions,
            EaseMode ease = EaseMode.Cubic)
        {
            if (from == null)
                throw new ArgumentNullException(nameof(from));

            if (to == null)
                throw new ArgumentNullException(nameof(to));

            this.ease = ease;
            this.transitions = transitions.Validate(from, to);
            start = from;
            end = to;
            offset = GetOffset();
            gameObject = CreateGameObject();
        }

        public void Dispose()
        {
            gameObject.Dispose(urgent: true);
        }


        public void Apply(float t)
        {
            var eased = ease.Apply(t);
            var easeOut = 1 - Mathf.Clamp(t * 2, 0, 1);
            var easeIn = Mathf.Clamp(t * 2 - 1, 0, 1);

            foreach (var (groupTransform, group) in remove)
                groupTransform.localScale = group.localScale * easeOut;

            foreach (var (groupTransform, group) in add)
                groupTransform.localScale = group.localScale * easeIn;

            foreach (var (groupTransform, before, after) in transition) {
                groupTransform.localScale = Vector3.Lerp(before.localScale, after.localScale, eased);
                groupTransform.localPosition = Vector3.Lerp(before.localPosition, after.localPosition + offset, eased);
            }
        }

        private IEnumerable<Transform> GroupsToAdd()
        {
            var endCursor = 0;
            var endGroups = end.transform.GetChildren();

            for (var i = 0; i < transitions.Length; i++) {
                if (transitions[i] is TransitionType.Add or TransitionType.Replace) {
                    if (endGroups.Length > endCursor)
                        yield return endGroups[endCursor];
                }

                if (transitions[i] is not TransitionType.Remove)
                    endCursor++;
            }
        }

        private IEnumerable<Transform> GroupsToRemove()
        {
            var startCursor = 0;
            var startGroups = start.transform.GetChildren();

            for (var i = 0; i < transitions.Length; i++) {
                if (transitions[i] is TransitionType.Remove or TransitionType.Replace) {
                    if (startGroups.Length > startCursor)
                        yield return startGroups[startCursor];
                }

                if (transitions[i] is not TransitionType.Add)
                    startCursor++;
            }
        }

        private IEnumerable<(Transform, Transform)> GetCommonGroups()
        {
            var startCursor = 0;
            var endCursor = 0;
            var startGroups = start.transform.GetChildren();
            var endGroups = end.transform.GetChildren();

            for (var i = 0; i < transitions.Length; i++) {
                if (transitions[i] is not TransitionType.Add and not TransitionType.Remove and not TransitionType.Replace) {
                    if (startGroups.Length > startCursor && endGroups.Length > endCursor)
                        yield return (startGroups[startCursor], endGroups[endCursor]);
                }

                if (transitions[i] is not TransitionType.Add)
                    startCursor++;

                if (transitions[i] is not TransitionType.Remove)
                    endCursor++;
            }
        }

        public Vector3 GetOffset() => GetOffsetWith(start, end, transitions);
        public static Vector3 GetOffsetWith(LatexComponent from, LatexComponent to, params TransitionType[] transitions)
        {
            var startCursor = 0;
            var endCursor = 0;

            for (var i = 0; i < transitions.Length; i++) {
                if (transitions[i] is TransitionType.Anchor) {
                    var startGroup = from.transform.GetChild(startCursor);
                    var endGroup = to.transform.GetChild(endCursor);
                    return startGroup.localPosition - endGroup.localPosition;
                }

                if (transitions[i] is not TransitionType.Add)
                    startCursor++;

                if (transitions[i] is not TransitionType.Remove)
                    endCursor++;
            }

            return Vector3.zero;
        }

        #region Initialization
        private GameObject CreateGameObject()
        {
            var container = new GameObject($"LatexTransition {instances++}") {
                hideFlags = HideFlags.DontSave,
            };

            PrimerTimeline.MarkAsEphemeral(container);
            CreateTransforms(container.transform);
            return container;
        }

        private void CreateTransforms(Transform parent)
        {
            foreach (var (before, after) in GetCommonGroups()) {
                var groupTransform = Object.Instantiate(before, parent);
                groupTransform.localPosition += offset;
                transition.Add((groupTransform, before, after));
            }

            foreach (var group in GroupsToRemove()) {
                var groupTransform = Object.Instantiate(group, parent);
                groupTransform.localScale = Vector3.zero;
                remove.Add((groupTransform, group));
            }

            foreach (var group in GroupsToAdd()) {
                var groupTransform = Object.Instantiate(group, parent);
                groupTransform.localPosition += offset;
                groupTransform.localScale = Vector3.zero;
                add.Add((groupTransform, group));
            }
        }
        #endregion
    }
}
