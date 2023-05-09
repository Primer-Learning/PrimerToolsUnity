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
        private static readonly int[] empty = Array.Empty<int>();

        private readonly LatexComponent before;
        private readonly LatexComponent after;
        private readonly List<GroupTransitionType> transitions;
        private List<int> fromGroups;
        private List<int> toGroups;

        public MutableStage(LatexComponent before, Stage stage)
        {
            if (before == null || stage.latex == null || stage.transition is null)
                throw new ArgumentException("Invalid stage");

            stage.latex.ConnectParts();

            this.before = before;
            after = stage.latex;
            transitions = stage.transition.ToList();
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
            var (i, from, _) = GetIndexesAt(index);
            if (from == null) return;

            fromGroups ??= before.groupIndexes.ToList();
            fromGroups.Insert(from.Value , splitIndex);

            var newTransition = GetTransitionAfter(i) == GroupTransitionType.Add
                ? GroupTransitionType.Transition
                : GroupTransitionType.Remove;

            transitions.Insert(index + 1, newTransition);
        }

        public void AddGroupAfter(int index, int splitIndex)
        {
            var (i, _, to) = GetIndexesAt(index);
            if (to == null) return;

            toGroups ??= after.groupIndexes.ToList();
            toGroups.Insert(to.Value , splitIndex);

            var newTransition = GetTransitionAfter(i) == GroupTransitionType.Remove
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
            var (_, from, to) = GetIndexesAt(index);
            transitions.RemoveAt(index);

            if (from != null) {
                fromGroups ??= before.groupIndexes.ToList();
                fromGroups.RemoveAt(from.Value - 1);
            }

            if (to != null) {
                toGroups ??= after.groupIndexes.ToList();
                toGroups.RemoveAt(to.Value - 1);
            }
        }

        private (int, int?, int?) GetIndexesAt(int index)
        {
            var enumerator = transitions.GetGroupIndexes();

            for (var i = 0; i <= index; i++) {
                if (!enumerator.MoveNext())
                    throw new IndexOutOfRangeException($"Index out of range {index}");
            }

            return enumerator.Current;
        }

        public Stage ToStage(Stage previously)
        {
            if (transitions.SequenceEqual(previously.transition)
                && (fromGroups is null || fromGroups.SequenceEqual(previously.groupIndexesBefore ?? empty))
                && (toGroups is null || toGroups.SequenceEqual(previously.groupIndexesAfter ?? empty))) {
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
            var fromRanges = before.expression.CalculateRanges(fromGroups ?? before.groupIndexes);
            var toRanges = after.expression.CalculateRanges(toGroups ?? after.groupIndexes);

            transitions.AdjustLength(fromRanges.Count, toRanges.Count);

            var enumerator = transitions.GetGroupIndexes();
            while (enumerator.MoveNext()) {
                var (i, from, to) = enumerator.Current;

                yield return new GroupTransition(
                    i,
                    transitions[i],
                    from is null ? null : fromRanges[from.Value],
                    to is null ? null : toRanges[to.Value]
                );
            }
        }
    }
}
