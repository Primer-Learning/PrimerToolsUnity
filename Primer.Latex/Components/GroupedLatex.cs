using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Latex
{
    [ExecuteAlways]
    [RequireComponent(typeof(LatexComponent))]
    public class GroupedLatex : MonoBehaviour, IReadOnlyList<Transform>, IHierarchyManipulator
    {
        #region IReadOnlyList implementation
        private List<Transform> groups = new();
        public int Count => groups.Count;
        public Transform this[int index] => groups[index];
        public IEnumerator<Transform> GetEnumerator() => groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion;

        #region public Transform root { get; }
        private Transform rootCache;
        internal Transform root => Meta.CachedChildFind(ref rootCache, transform, $"Grouping > {groupName}");
        #endregion

        #region public LatexComponent latex { get; }
        private LatexComponent latexCache;
        private LatexComponent latex => latexCache ??= GetComponent<LatexComponent>();
        #endregion

        #region public LatexExpression expression { get; set; } = latex.expression;
        private LatexExpression customExpression;

        [SerializeField, HideInInspector]
        private bool hasCustomExpression = false;

        public LatexExpression expression {
            get {
                if (!hasCustomExpression)
                    return latex.expression;

                customExpression ??= LatexExpression.FromHierarchy(root);
                return customExpression;
            }
            set {
                customExpression = value;
                hasCustomExpression = true;
                UpdateChildren();
            }
        }
        #endregion

        #region public string groupName;
        [SerializeField, HideInInspector]
        private string _groupName;

        [ShowInInspector, Required]
        public string groupName {
            get => _groupName;
            set {
                root.name = $"Grouping > {value}";
                _groupName = value;
            }
        }
        #endregion

        #region public List<int> groupIndexes;
        [SerializeField, HideInInspector]
        // TODO: This has to be edited from the editor!
        private List<int> _groupIndexes = new();

        [ShowInInspector]
        public List<int> groupIndexes {
            get => _groupIndexes;
            set {
                _groupIndexes = value;
                UpdateChildren();
            }
        }
        #endregion

        public Action onGroupsChange;

        private void OnEnable() => latex.onExpressionChange += HandleLatexComponentChange;
        private void OnDisable() => latex.onExpressionChange -= HandleLatexComponentChange;

        private void HandleLatexComponentChange()
        {
            if (customExpression is null)
                UpdateChildren();
        }

        public void UpdateChildren()
        {
            var currentExpression = expression;

            if (currentExpression is null) {
                Debug.LogError("Cannot create LatexGroup, expression is null!");
                return;
            }

            if (currentExpression.Any(x => x.mesh is null)) {
                Debug.LogError("Cannot create LatexGroup, expression lacks meshes! 😱");
                return;
            }

            // Groups are only used in transitions, we don't need to see them
            var container = new Container(root, setActive: false);
            var zero = currentExpression.center;
            var currentMaterial = latex.material;
            var currentColor = latex.color;

            groups.Clear();

            foreach (var (groupIndex, (start, end)) in CalculateRanges().WithIndex()) {
                var chunk = currentExpression.Slice(start, end);
                var center = chunk.center;
                var group = container.AddContainer($"Group {groupIndex} ({start} to {end - 1})");

                group.transform.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.transform.localScale = Vector3.one;
                group.transform.localRotation = Quaternion.identity;

                foreach (var (index, character) in chunk.WithIndex()) {
                    var charTransform = group.Add($"LatexChar {start + index}").SetDefaults();
                    character.RenderTo(charTransform, currentMaterial, currentColor);
                    charTransform.localPosition -= group.transform.localPosition;
                }

                groups.Add(group.transform);
            }

            container.Purge();
            onGroupsChange?.Invoke();
        }

        public void RegenerateChildren()
        {
            root.RemoveAllChildren();
            UpdateChildren();
        }

        #region Code to clipboard
        [CopyToClipboard]
        public string ToCode()
        {
            return $".Add<{nameof(GroupedLatex)}>(){SetInvoker()}";
        }

        [CopyToClipboard("Copy code w/ LatexComponent")]
        public string ToCodeWithLatex()
        {
            return latex.ToCode() + $".GetOrAddComponent<{nameof(GroupedLatex)}>(){SetInvoker()}";
        }

        internal string SetInvoker()
        {
            return $".Set(\"{groupName}\", {groupIndexes.Select(x => x.ToString()).Join(", ")})";
        }

        public GroupedLatex Set(string groupName, IEnumerable<int> groupIndexes, LatexExpression expression = null)
        {
            customExpression = expression;
            hasCustomExpression = expression is not null;
            this.groupName = groupName;
            this.groupIndexes = groupIndexes.ToList();
            UpdateChildren();
            return this;
        }
        #endregion

        internal IEnumerable<(int, int)> CalculateRanges() => CalculateRanges(expression, groupIndexes);

        public static IEnumerable<(int, int)> CalculateRanges(LatexExpression expression, IEnumerable<int> groupIndexes = null)
        {
            var last = 0;
            var result = new List<(int, int)>();
            var indexes = groupIndexes ?? Array.Empty<int>();

            foreach (var start in indexes) {
                if (start == last)
                    continue;

                if (start >= expression.Count)
                    break;

                result.Add((last, start));
                last = start;
            }

            if (last != expression.Count)
                result.Add((last, expression.Count));

            return result.ToArray();
        }
    }
}
