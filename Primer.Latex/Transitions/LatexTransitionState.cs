using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Primer.Latex
{
    internal class LatexTransitionState
    {
        private static void EnsureGroupsHaveSameLength<T, U>(IEnumerable<T> left, IEnumerable<U> right)
            => EnsureGroupsHaveSameLength(left.Count(), right.Count());

        private static void EnsureGroupsHaveSameLength(int left, int right)
        {
            const string ERROR_MESSAGE = "Can't transition from states with different amount of groups";
            Assert.AreEqual(left, right, ERROR_MESSAGE);
        }

        private readonly GroupState[] groups;

        public LatexTransitionState(Transform transform, IEnumerable<LatexExpression> groups)
        {
            this.groups = groups.Select((_, i) => new GroupState(transform, i)).ToArray();
        }

        public LatexTransitionState(Transform transform, IReadOnlyCollection<TransitionType> transitions)
        {
            // EnsureGroupsHaveSameLength(transform.childCount, transitions.Count);
            groups = transitions.Select((x, i) => new GroupState(transform, i, x)).ToArray();
        }

        public IEnumerable<GroupState> GroupsToRemoveTransitioningTo(LatexTransitionState other)
        {
            EnsureGroupsHaveSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                var group = groups[i];
                var otherGroup = other.groups[i];

                if (group.isReplaced || otherGroup.isReplaced || group.isRemoved && !other.groups[i].isRemoved)
                    yield return group;
            }
        }

        public IEnumerable<(GroupState, GroupState)> GetCommonGroups(LatexTransitionState other)
        {
            EnsureGroupsHaveSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                if (groups[i].isLerped && other.groups[i].isLerped)
                    yield return (groups[i], other.groups[i]);
            }
        }
    }
}
