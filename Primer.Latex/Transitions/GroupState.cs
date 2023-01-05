using UnityEngine;

namespace Primer.Latex
{
    internal class GroupState
    {
        private readonly int childIndex;
        private readonly Transform parent;
        private readonly TransitionType transition;

        public GroupState(Transform parent, int childIndex,
            TransitionType transition = TransitionType.Transition)
        {
            this.parent = parent;
            this.childIndex = childIndex;
            this.transition = transition;
        }

        public Transform transform => parent.GetChild(childIndex);
        public Vector3 scale => transform.localScale;
        public Vector3 position => transform.localPosition;

        public bool isLerped => transition == TransitionType.Transition;
        public bool isRemoved => transition == TransitionType.Remove;
        public bool isReplaced => transition == TransitionType.Replace;
    }
}
