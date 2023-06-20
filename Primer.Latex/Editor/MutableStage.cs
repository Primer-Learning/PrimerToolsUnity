using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Stage = Primer.Latex.LatexSequence.Stage;
using GroupTransition = System.Tuple<
    int,
    Primer.Latex.GroupTransitionType,
    (int start, int end)?,
    (int start, int end)?
>;

namespace Primer.Latex.Editor
{
    internal class MutableStage : IEnumerable<GroupTransition>
    {
        private static List<int> CreateEmptyIndexList() => new List<int>();

        private readonly LatexComponent before;
        private readonly LatexComponent after;
        private TransitionList transitions;
        private List<int> fromGroups;
        private List<int> toGroups;

        public MutableStage(LatexComponent before, Stage stage)
        {
            if (before == null || stage.latex == null || stage.transition is null)
                throw new ArgumentException("Invalid stage");

            this.before = before;
            after = stage.latex;
            transitions = new TransitionList(stage.transition);
            fromGroups = stage.groupIndexesBefore?.ToList();
            toGroups = stage.groupIndexesAfter?.ToList();
        }

        public void SetTransitionType(int index, GroupTransitionType newTransition)
        {
            transitions[index] = newTransition;
        }

        public LatexExpression GetGroupBefore((int start, int end) range)
        {
            return before.expression.Slice(range.start, range.end);
        }

        public LatexExpression GetGroupAfter((int start, int end) range)
        {
            return after.expression.Slice(range.start, range.end);
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
        }

        private TransitionList.Entry GetIndexesAt(int index)
        {
            return transitions.GetIndexes().Skip(index).First();
        }

        public Stage ToStage(Stage previously)
        {
            if (transitions.SequenceEqual(previously.transition)
                && fromGroups.IsSame(previously.groupIndexesBefore)
                && toGroups.IsSame(previously.groupIndexesAfter)) {
                return previously;
            }

            return new Stage(
                previously.latex,
                transitions.ToArray(),
                fromGroups?.ToArray(),
                toGroups?.ToArray()
            );
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<GroupTransition> GetEnumerator()
        {
            var fromRanges = GroupedLatex.CalculateRanges(before.expression, fromGroups).ToList();
            var toRanges = GroupedLatex.CalculateRanges(after.expression, toGroups).ToList();

            transitions = transitions.For(fromRanges.Count, toRanges.Count);

            foreach (var (type, index, from, to) in transitions.GetIndexes()) {
                yield return new GroupTransition(
                    index,
                    type,
                    from.HasValue ? fromRanges[from.Value] : null,
                    to.HasValue ? toRanges[to.Value] : null
                );
            }
        }
    }
}
