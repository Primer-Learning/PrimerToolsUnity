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
        public LatexComponent latexCache;
        public LatexComponent latex => latexCache ??= GetComponent<LatexComponent>();
        #endregion

        #region public LatexExpression expression { get; set; } = latex.expression;
        public LatexExpression customExpression = null;
        public LatexExpression expression {
            get => customExpression ?? latex.expression;
            set {
                customExpression = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public List<int> groupIndexes;
        [SerializeField]
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

            groups.Clear();

            foreach (var (start, end) in CalculateRanges()) {
                var chunk = currentExpression.Slice(start, end);
                var center = chunk.center;
                var group = container.AddContainer($"Group (chars {start} to {end - 1})");

                group.transform.localPosition = Vector3.Scale(center - zero, new Vector3(1, -1, 1));
                group.transform.localScale = Vector3.one;
                group.transform.localRotation = Quaternion.identity;

                foreach (var character in chunk) {
                    var mesh = group.AddContainer<MeshFilter>($"LatexChar {character.position}");
                    mesh.component.sharedMesh = character.mesh;
                    mesh.transform.localScale = Vector3.one;
                    mesh.transform.localPosition = character.position - group.transform.localPosition;
                    mesh.transform.localRotation = Quaternion.identity;
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

        private IEnumerable<(int, int)> CalculateRanges()
        {
            var last = 0;
            var result = new List<(int, int)>();

            foreach (var start in groupIndexes) {
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
