using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Latex
{
    [RequireComponent(typeof(LatexComponent))]
    public class GroupedLatex : MonoBehaviour, IReadOnlyList<Transform>
    {
        [Required] public string groupName;

        #region IReadOnlyList implementation
        private List<Transform> groups = new();
        public int Count => groups.Count;
        public Transform this[int index] => groups[index];
        public IEnumerator<Transform> GetEnumerator() => groups.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion;

        #region public Transform root { get; }
        private Transform rootCache;
        public Transform root => rootCache ??= transform.FindOrCreate($"Grouping_{groupName}");
        #endregion

        #region public LatexComponent latex { get; }
        private LatexComponent latexCache;
        public LatexComponent latex => latexCache ??= GetComponent<LatexComponent>();
        #endregion

        #region public LatexExpression expression { get; set; } = latex.expression;
        [SerializeField, HideInInspector]
        private LatexExpression customExpression;
        [SerializeField, HideInInspector]
        private bool hasCustomExpression = false;

        public LatexExpression expression {
            get => hasCustomExpression ? customExpression ?? latex.expression : latex.expression;
            set {
                customExpression = value;
                hasCustomExpression = true;
                UpdateChildren();
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

        [Button("Update groups")]
        private void UpdateChildren()
        {
            var currentExpression = this.expression;

            if (currentExpression is null) {
                Debug.LogError("Cannot create LatexGroup, expression is null!");
                return;
            }

            if (currentExpression.Any(x => x.mesh is null)) {
                Debug.LogError("Cannot create LatexGroup, expression lacks meshes! 😱");
                return;
            }

            var zero = currentExpression.center;
            var container = new Container(root);
            var currentMaterial = latex.material;
            var currentColor = latex.color;

            groups.Clear();

            foreach (var (start, end) in CalculateRanges()) {
                var chunk = currentExpression.Slice(start, end);
                var center = chunk.center;
                var group = container.AddContainer($"Group (chars {start} to {end - 1})");

                group.transform.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.transform.localScale = Vector3.one;
                group.transform.localRotation = Quaternion.identity;

                foreach (var (index, character) in chunk.WithIndex()) {
                    var charTransform = container.Add($"LatexChar {start + index}").SetDefaults();
                    character.RenderTo(charTransform, currentMaterial, currentColor);
                    charTransform.localPosition -= group.transform.localPosition;
                }

                groups.Add(group.transform);
            }

            container.Purge();
            onGroupsChange?.Invoke();
        }

        [Button("Regenerate children")]
        public void DestroyAndUpdate()
        {
            root.RemoveAllChildren();
            UpdateChildren();
        }

        #region Copy code
        [CopyCode]
        public string ToCode()
        {
            return $".Add<{nameof(GroupedLatex)}>(){SetInvoker()}";
        }

        [CopyCode("Copy code w/ LatexComponent")]
        public string CopyCodeWithLatexComponent()
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
            this.groupName = groupName;
            this.groupIndexes = groupIndexes.ToList();
            // Implicit when we set group indexes
            // UpdateChildren();
            return this;
        }
        #endregion

        private IEnumerable<(int, int)> CalculateRanges() => CalculateRanges(expression, groupIndexes);

        public static IEnumerable<(int, int)> CalculateRanges(LatexExpression expression, IEnumerable<int> groupIndexes = null)
        {
            var last = 0;
            var result = new List<(int, int)>();
            var indexes = groupIndexes ?? Array.Empty<int>();

            foreach (var start in indexes) {
                if (start == last)
                    continue;

                if (start >= expression.count)
                    break;

                result.Add((last, start));
                last = start;
            }

            if (last != expression.count)
                result.Add((last, expression.count));

            return result.ToArray();
        }
    }
}
