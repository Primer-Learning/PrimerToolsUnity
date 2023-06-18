using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Primer.Latex
{
    internal class TransitionList : IReadOnlyList<GroupTransitionType>
    {
        public record TransitionEntry(GroupTransitionType type, int index, int? fromIndex, int? toIndex)
        {
            public bool isAnchor => type == GroupTransitionType.Anchor;
            public bool isTransition => type == GroupTransitionType.Transition;
            public bool isReplace => type == GroupTransitionType.Replace;
            public bool isRemove => type == GroupTransitionType.Remove;
            public bool isAdd => type == GroupTransitionType.Add;
        }

        private readonly IReadOnlyList<GroupTransitionType> transitions;

        public int Count => transitions.Count;
        public GroupTransitionType this[int index] => transitions[index];


        public TransitionList(IReadOnlyList<GroupTransitionType> transitions)
        {
            this.transitions = transitions;
        }


        public IEnumerable<TransitionEntry> GetIndexes()
        {
            var fromCursor = 0;
            var toCursor = 0;

            for (var i = 0; i < transitions.Count; i++) {
                var transition = transitions[i];

                yield return new TransitionEntry(
                    transition,
                    i,
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

        public TransitionList For(GroupedExpression from, GroupedExpression to)
        {
            return Validate().AdjustLength(from, to);
        }

        public TransitionList AdjustLength(GroupedExpression from, GroupedExpression to)
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
