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
        public const string ROOT_NAME = "Transition";

        public Transform root => transform.FindOrCreate(ROOT_NAME);

        #region LatexComponent latex { get; }
        private LatexComponent latexCache;
        public LatexComponent latex => latexCache ??= GetComponent<LatexComponent>();
        #endregion

        [SerializeField] private GroupedLatex start;
        [SerializeField] private GroupedLatex end;
        [SerializeField] private TransitionList transitions;
        [SerializeField] private Vector3 offset;

        private readonly List<(Transform mid, Transform to)> add = new();
        private readonly List<(Transform mid, Transform from)> remove = new();
        private readonly List<(Transform mid, Transform from, Transform to)> transition = new();
        private readonly List<(Transform mid, Transform from, Transform to)> replace = new();

        #region Copy code
        [CopyCode]
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

        [CopyCode("Copy code w/ LatexComponent")]
        public string CopyCodeWithLatexComponent()
        {
            return latex.ToCode() + ToCode();
        }

        public LatexTransition Set(GroupedLatex start, GroupedLatex end, TransitionList transitions)
        {
            this.start = start;
            this.end = end;
            this.transitions = transitions.For(start, end);
            offset = CalculateOffset();
            start.onGroupsChange += UpdateChildren;
            end.onGroupsChange += UpdateChildren;
            UpdateChildren();
            return this;
        }
        #endregion

        public Tween ToTween()
        {
            return new Tween(Evaluate).Observe(
                whenZero: SetInitialState,
                whenOne: SetEndState
            );
        }

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

            var easeOut = 1 - Mathf.Clamp(t * 2, 0, 1);
            var easeIn = Mathf.Clamp(t * 2 - 1, 0, 1);

            foreach (var (groupTransform, group) in remove)
                groupTransform.localScale = group.localScale * easeOut;

            foreach (var (groupTransform, group) in add)
                groupTransform.localScale = group.localScale * easeIn;

            foreach (var (groupTransform, before, after) in transition) {
                groupTransform.localScale = Vector3.Lerp(before.localScale, after.localScale, t);
                groupTransform.localPosition = Vector3.Lerp(before.localPosition, after.localPosition + offset, t);
            }

            foreach (var (groupTransform, before, after) in replace) {
                var isFirstHalf = easeIn <= 0;
                var first = groupTransform.GetChild(0);
                var second = groupTransform.GetChild(1);

                first.SetActive(isFirstHalf);
                second.SetActive(!isFirstHalf);

                groupTransform.localPosition = Vector3.Lerp(before.localPosition, after.localPosition + offset, t);

                groupTransform.localScale = isFirstHalf
                    ? before.localScale * easeOut
                    : after.localScale * easeIn;
            }
        }

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
            replace.Clear();
            remove.Clear();
            add.Clear();

            Deactivate();

            var container = new Container(root);

            foreach (var (before, after) in GetCommonGroups()) {
                var groupTransform = container.Add(before);
                groupTransform.localPosition += offset;
                transition.Add((groupTransform, before, after));
            }

            foreach (var (before, after) in GroupToReplace()) {
                var group = container.AddContainer("Replace");
                group.localPosition += before.localPosition + offset;
                group.Add(before).localPosition = Vector3.zero;
                group.Add(after).localPosition = Vector3.zero;
                replace.Add((group.transform, before, after));
            }

            foreach (var group in GroupsToRemove()) {
                var groupTransform = container.Add(group);
                groupTransform.localScale = Vector3.zero;
                remove.Add((groupTransform, group));
            }

            foreach (var group in GroupsToAdd()) {
                var groupTransform = container.Add(group);
                groupTransform.localPosition += offset;
                groupTransform.localScale = Vector3.zero;
                add.Add((groupTransform, group));
            }

            container.Purge();
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
