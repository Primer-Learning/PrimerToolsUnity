using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

using GroupTransition = System.Tuple<
    int,
    Primer.Latex.GroupTransitionType,
    (int start, int end)?,
    (int start, int end)?
>;

namespace Primer.Latex.Editor
{
    internal class MutableTransitions : IEnumerable<GroupTransition>
    {
        private static List<int> CreateEmptyIndexList() => new();

        private readonly LatexExpression before;
        private readonly LatexExpression after;
        private TransitionList transitions;
        private List<int> fromGroups;
        private List<int> toGroups;

        public bool hasChanges { get; private set; }

        public MutableTransitions(LatexExpression from, IEnumerable<int> fromGroups, LatexExpression to,
            IEnumerable<int> toGroups, TransitionList transitions)
        {
            if (from == null || to == null || transitions is null)
                throw new ArgumentException("Invalid stage");

            before = from;
            after = to;
            this.fromGroups = fromGroups.ToList();
            this.toGroups = toGroups.ToList();
            this.transitions = transitions;
        }

        public void SetTransitionType(int index, GroupTransitionType newTransition)
        {
            transitions[index] = newTransition;
            hasChanges = true;
        }

        public LatexExpression GetGroupBefore((int start, int end) range)
        {
            return before.Slice(range.start, range.end);
        }

        public LatexExpression GetGroupAfter((int start, int end) range)
        {
            return after.Slice(range.start, range.end);
        }

        public void AddGroupBefore(int index, int splitIndex)
        {
            var entry = GetIndexesAt(index);
            if (entry.fromIndex == null) return;

            fromGroups ??= CreateEmptyIndexList();
            fromGroups.Insert(entry.fromIndex.Value , splitIndex);

            var newTransition = GetTransitionAfter(entry.index) == GroupTransitionType.Add
                ? GroupTransitionType.Transition
                : GroupTransitionType.Remove;

            transitions.Insert(index + 1, newTransition);
            hasChanges = true;
        }

        public void AddGroupAfter(int index, int splitIndex)
        {
            var entry = GetIndexesAt(index);
            if (entry.toIndex == null) return;

            toGroups ??= CreateEmptyIndexList();
            toGroups.Insert(entry.toIndex.Value , splitIndex);

            var newTransition = GetTransitionAfter(entry.index) == GroupTransitionType.Remove
                ? GroupTransitionType.Transition
                : GroupTransitionType.Add;

            transitions.Insert(index + 1, newTransition);
            hasChanges = true;
        }

        private GroupTransitionType? GetTransitionAfter(int index)
        {
            return transitions.Count <= index + 1 ? null : transitions[index + 1];
        }

        public void RemoveGroup(int index)
        {
            var entry = GetIndexesAt(index);
            transitions.RemoveAt(entry.index);

            if (entry.fromIndex.HasValue) {
                fromGroups ??= CreateEmptyIndexList();
                fromGroups.RemoveAt(entry.fromIndex.Value - 1);
            }

            if (entry.toIndex.HasValue) {
                toGroups ??= new List<int>();
                toGroups.RemoveAt(entry.toIndex.Value - 1);
            }

            hasChanges = true;
        }

        private TransitionList.Entry GetIndexesAt(int index)
        {
            return transitions.GetIndexes().Skip(index).First();
        }

        public (List<int>, List<int>, TransitionList) GetResult()
        {
            return (fromGroups, toGroups, transitions);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<GroupTransition> GetEnumerator()
        {
            var fromRanges = GroupedLatex.CalculateRanges(before, fromGroups).ToList();
            var toRanges = GroupedLatex.CalculateRanges(after, toGroups).ToList();

            var newTransitions = transitions.For(fromRanges.Count, toRanges.Count);

            if (newTransitions != transitions) {
                hasChanges = true;
                transitions = newTransitions;
            }

            foreach (var (type, index, from, to) in newTransitions.GetIndexes()) {
                yield return new GroupTransition(
                    index,
                    type,
                    from.HasValue ? fromRanges[from.Value] : null,
                    to.HasValue ? toRanges[to.Value] : null
                );
            }
        }

        public void RenderEditor()
        {
            SirenixEditorGUI.Title("Transition", "", TextAlignment.Center, false);
            LatexCharEditor.CharPreviewSize();

            var width = LatexCharEditor.GetDefaultWidth();
            var forceAnchor = -1;

            foreach (var (i, transition, fromRange, toRange) in this) {
                EditorGUILayout.Space(4);

                using (new GUILayout.HorizontalScope()) {
                    var newValue = EditorGUILayout.EnumPopup("", transition);
                    var newTransition = newValue is GroupTransitionType kind ? kind : transition;

                    if (newTransition is GroupTransitionType.Anchor && transition is not GroupTransitionType.Anchor)
                        forceAnchor = i;

                    if (newTransition != transition)
                        SetTransitionType(i, newTransition);

                    if (i != 0 && GUILayout.Button("X", GUILayout.Width(20))) {
                        RemoveGroup(i);
                        break;
                    }
                }

                if (fromRange.HasValue) {
                    var clicked = LatexCharEditor.ShowGroup(GetGroupBefore(fromRange.Value), width);

                    if (clicked is not 0)
                        AddGroupBefore(i, fromRange.Value.start + clicked);
                }

                if (fromRange.HasValue && toRange.HasValue)
                    EditorGUILayout.Space(4);

                if (toRange.HasValue) {
                    var clicked = LatexCharEditor.ShowGroup(GetGroupAfter(toRange.Value), width);

                    if (clicked is not 0)
                        AddGroupAfter(i, toRange.Value.start + clicked);
                }
            }

            if (forceAnchor is -1)
                return;

            foreach (var (i, transition, _, _) in this) {
                if (transition is GroupTransitionType.Anchor && i != forceAnchor)
                    SetTransitionType(i, GroupTransitionType.Transition);
            }
        }
    }
}
