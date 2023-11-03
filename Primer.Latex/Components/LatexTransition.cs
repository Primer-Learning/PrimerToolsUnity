using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using UnityEngine;

namespace Primer.Latex
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LatexComponent))]
    [RequireComponent(typeof(GroupedLatex))]
    public class LatexTransition : MonoBehaviour, IHierarchyManipulator
    {
        private const string ROOT_NAME = "Transition";

        #region public Transform root { get; }
        private Transform rootCache;
        private Transform root => Meta.CachedChildFind(ref rootCache, transform, ROOT_NAME);
        #endregion

        #region LatexComponent latex { get; }
        private LatexComponent latexCache;
        private LatexComponent latex => latexCache ??= GetComponent<LatexComponent>();
        #endregion

        [SerializeField] internal GroupedLatex start;
        [SerializeField] internal GroupedLatex end;

        [SerializeField, HideInInspector]
        private Vector3 offset;
        [SerializeField, HideInInspector]
        internal TransitionList transitions;

        private readonly List<(Transform mid, Transform to)> add = new();
        private readonly List<(Transform mid, Transform from)> remove = new();
        private readonly List<(Transform mid, Transform from, Transform to)> transition = new();

        #region Copy code
        [CopyToClipboard]
        public string ToCode()
        {
            return $@"
.Transition(
    from: ""{start.expression.source}""
    fromGroups: new int[] {{{start.groupIndexes.Join(", ")}}},
    to: ""{end.expression.source}"",
    toGroups: new int[] {{{end.groupIndexes.Join(", ")}}},
    {transitions.Select(x => x.ToCode()).Join(",\n    ")}
);
            ".Trim();
        }

        [CopyToClipboard("Copy code w/ LatexComponent")]
        public string ToCodeWithLatex()
        {
            return latex.ToCode() + ToCode();
        }

        public Vector3 Set(GroupedLatex start, GroupedLatex end, TransitionList transitions)
        {
            this.start = start;
            this.end = end;
            this.transitions = transitions.For(start, end);
            offset = CalculateOffset();
            start.onGroupsChange += UpdateChildren;
            end.onGroupsChange += UpdateChildren;
            UpdateChildren();
            return offset;
        }
        #endregion

        public Tween ToTween()
        {
            return new Tween(Evaluate).Observe(
                onStart: SetInitialState,
                onComplete: SetEndState
            );
        }

        #region Transition stages
        public void Deactivate()
        {
            root.SetActive(false);
            latex.ResetActiveDisplay();
        }

        public void SetInitialState()
        {
            latex.SetActiveDisplay(start.root);
        }

        public void SetEndState()
        {
            latex.SetActiveDisplay(end.root);
        }

        public void Evaluate(float t)
        {
            latex.SetActiveDisplay(root);

            if (remove.Count + add.Count + transition.Count == 0) {
                UpdateChildren();
            }

            var easeOut = 1 - Mathf.Clamp(t * 2, 0, 1);
            var easeIn = Mathf.Clamp(t * 2 - 1, 0, 1);

            foreach (var (groupTransform, group) in remove)
                groupTransform.localScale = group.localScale * easeOut;

            foreach (var (groupTransform, group) in add)
                groupTransform.localScale = group.localScale * easeIn;

            foreach (var (groupTransform, before, after) in transition)
                groupTransform.localPosition = Vector3.Lerp(before.localPosition, after.localPosition + offset, t);
        }
        #endregion

        public Vector3 CalculateOffset()
        {
            if (start is null || end is null)
                return Vector3.zero;

            var anchor = transitions.GetIndexes().FirstOrDefault(x => x.type is GroupTransitionType.Anchor);

            return anchor is null || anchor.fromIndex is null || anchor.toIndex is null
                ? Vector3.zero
                : start[anchor.fromIndex.Value].localPosition - end[anchor.toIndex.Value].localPosition;
        }

        #region Object creation
        public void UpdateChildren()
        {
            transition.Clear();
            remove.Clear();
            add.Clear();

            Deactivate();

            var gnome = new Gnome(root);

            foreach (var group in GroupsToRemove()) {
                var groupTransform = gnome.Add(group);
                remove.Add((groupTransform, group));
            }


            foreach (var group in GroupsToAdd()) {
                var groupTransform = gnome.Add(group);
                groupTransform.localPosition += offset;
                add.Add((groupTransform, group));
            }

            foreach (var (before, after) in GetCommonGroups()) {
                var groupTransform = gnome.Add(before);
                transition.Add((groupTransform, before, after));
            }

            foreach (var (before, after) in GroupToReplace()) {
                var group = gnome.AddGnome("Replace");
                var scaleDown = group.Add(before);
                scaleDown.localPosition = Vector3.zero;
                var scaleUp = group.Add(after);
                scaleUp.localPosition = Vector3.zero;

                transition.Add((group, before, after));
                remove.Add((scaleDown, before));
                add.Add((scaleUp, after));
            }

            gnome.Purge();
        }

        public void RegenerateChildren()
        {
            root.RemoveAllChildren();
            UpdateChildren();
        }

        private IEnumerable<Transform> GroupsToAdd()
        {
            return transitions.GetIndexes()
                .Where(x => x.type is GroupTransitionType.Add)
                .Where(x => end.Count > x.toIndex)
                .Select(x => end[x.toIndex.Value]);
        }

        private IEnumerable<Transform> GroupsToRemove()
        {
            return transitions.GetIndexes()
                .Where(x => x.type is GroupTransitionType.Remove)
                .Where(x => start.Count > x.fromIndex)
                .Select(x => start[x.fromIndex.Value]);
        }

        private IEnumerable<(Transform, Transform)> GroupToReplace()
        {
            return transitions.GetIndexes()
                .Where(x => x.type is GroupTransitionType.Replace)
                .Where(x => start.Count > x.fromIndex && end.Count > x.toIndex)
                .Select(x => (start[x.fromIndex.Value], end[x.toIndex.Value]));
        }

        private IEnumerable<(Transform, Transform)> GetCommonGroups()
        {
            return transitions.GetIndexes()
                .Where(x => x.type is GroupTransitionType.Transition or GroupTransitionType.Anchor)
                .Where(x => start.Count > x.fromIndex && end.Count > x.toIndex)
                .Select(x => (start[x.fromIndex.Value], end[x.toIndex.Value]));
        }
        #endregion
    }
}
