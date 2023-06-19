using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public class TransitionList : IReadOnlyList<GroupTransitionType>
    {
        public record TransitionEntry(GroupTransitionType type, int? fromIndex, int? toIndex);

        // This is readonly but that doesn't work with [SerializeField]
        [SerializeField]
        private List<GroupTransitionType> transitions;

        public int Count => transitions.Count;
        public GroupTransitionType this[int index] => transitions[index];


        public TransitionList(IEnumerable<GroupTransitionType> transitions)
        {
            this.transitions = transitions.ToList();
        }


        public IEnumerable<TransitionEntry> GetIndexes()
        {
            var fromCursor = 0;
            var toCursor = 0;

            for (var i = 0; i < transitions.Count; i++) {
                var transition = transitions[i];

                yield return new TransitionEntry(
                    transition,
                    transition is GroupTransitionType.Add ? null : fromCursor++,
                    transition is GroupTransitionType.Remove ? null : toCursor++
                );
            }
        }

        public TransitionList Validate()
        {
            if (transitions.Count(x => x == GroupTransitionType.Anchor) > 1)
                throw new Exception("Cannot have more than one anchor transition");

            return this;
        }

        public TransitionList For(GroupedLatex from, GroupedLatex to)
        {
            return Validate().AdjustLength(from, to);
        }

        public TransitionList AdjustLength(GroupedLatex from, GroupedLatex to)
            => AdjustLength(from.Count, to.Count);

        public TransitionList AdjustLength(int fromGroupsCount, int toGroupsCount)
        {
            var copy = transitions.ToList();
            var fromCursor = 0;
            var toCursor = 0;

            foreach (var transition in transitions) {
                if (transition is not GroupTransitionType.Add)
                    fromCursor++;

                if (transition is not GroupTransitionType.Remove)
                    toCursor++;
            }

            while (fromCursor < fromGroupsCount && toCursor < toGroupsCount) {
                copy.Add(GroupTransitionType.Transition);
                fromCursor++;
                toCursor++;
            }

            while (fromCursor < fromGroupsCount) {
                copy.Add(GroupTransitionType.Remove);
                fromCursor++;
            }

            while (toCursor < toGroupsCount) {
                copy.Add(GroupTransitionType.Add);
                toCursor++;
            }

            while (copy.Count is not 0 && fromCursor > fromGroupsCount) {
                var index = copy.LastIndexOf(GroupTransitionType.Remove);
                copy.RemoveAt(index == -1 ? copy.Count - 1 : index);
                fromCursor--;
            }

            while (copy.Count is not 0 && toCursor > toGroupsCount) {
                var index = copy.LastIndexOf(GroupTransitionType.Add);
                copy.RemoveAt(index == -1 ? copy.Count - 1 : index);
                toCursor--;
            }

            return new TransitionList(copy.ToArray());
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<GroupTransitionType> GetEnumerator() => transitions.GetEnumerator();
    }
}
