using UnityEngine;

namespace Primer.Latex
{
    internal class GroupState
    {
        private readonly int childIndex;
        private readonly Transform parent;
        private readonly TransitionType transition;

        public GroupState(Transform parent, int childIndex, TransitionType transition = TransitionType.Replace)
        {
            this.parent = parent;
            this.childIndex = childIndex;
            this.transition = transition;
        }

        public Transform transform => parent.GetChild(childIndex);
        public Vector3 scale => transform.localScale;
        public Vector3 position => transform.localPosition;

        public bool isAnchor => transition == TransitionType.Anchor;
        private bool isRemoved => transition == TransitionType.Remove;
        private bool isReplaced => transition == TransitionType.Replace;

        public bool IsAdded(GroupState to) => to.isReplaced || isRemoved;
        public bool IsRemoved(GroupState to) => to.isReplaced || to.isRemoved;
    }
}
