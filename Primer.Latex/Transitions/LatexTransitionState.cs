using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Primer.Latex
{
    internal class LatexTransitionState
    {
        private readonly GroupState[] groups;
        private readonly TransformSnapshot snapshot;
        private readonly LatexRenderer source;


        public Transform transform => source.transform;
        public GameObject gameObject => source.gameObject;


        public LatexTransitionState(LatexRenderer renderer, IEnumerable<LatexExpression> groups)
        {
            source = renderer;
            snapshot = new TransformSnapshot(source.transform);
            this.groups = groups.Select((_, i) => new GroupState(renderer.transform, i)).ToArray();
        }

        public LatexTransitionState(LatexRenderer renderer, IEnumerable<TransitionType> transitions)
        {
            source = renderer;
            snapshot = new TransformSnapshot(source.transform);
            groups = transitions.Select((x, i) => new GroupState(renderer.transform, i, x)).ToArray();
        }

        public void Restore() => snapshot.Restore();


        public IEnumerable<GroupState> GroupsToAddTransitioningTo(LatexTransitionState other)
        {
            AssertSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                var group = groups[i];
                var otherGroup = other.groups[i];

                if (group.IsAdded(otherGroup))
                    yield return otherGroup;
            }
        }

        public IEnumerable<GroupState> GroupsToRemoveTransitioningTo(LatexTransitionState other)
        {
            AssertSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                var group = groups[i];
                var otherGroup = other.groups[i];

                if (group.IsRemoved(otherGroup))
                    yield return group;
            }
        }

        public IEnumerable<(GroupState, GroupState)> GetCommonGroups(LatexTransitionState other)
        {
            AssertSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                var group = groups[i];
                var otherGroup = other.groups[i];

                if (!group.IsAdded(otherGroup) && !group.IsRemoved(otherGroup))
                    yield return (group, otherGroup);
            }
        }

        public Vector3 GetOffsetWith(LatexTransitionState other)
        {
            AssertSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                var group = groups[i];
                var otherGroup = other.groups[i];

                if (otherGroup.isAnchor)
                    return group.position - otherGroup.position;
            }

            return Vector3.zero;
        }

        private void AssertSameLength<TLeft, TRight>(IEnumerable<TLeft> left, IEnumerable<TRight> right)
            => AssertSameLength(left.Count(), right.Count());

        private void AssertSameLength(int left, int right)
        {
            try {
                Assert.AreEqual(left, right, "Can't transition from states with different amount of groups");
            }
            catch (AssertionException ex) {
                source.Error(ex);
            }
        }
    }
}
