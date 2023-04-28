using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Latex
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Groups")]
    internal class LatexGroups
    {
        [SerializeField]
        [HideInInspector]
        internal List<int> indexes = new();

        [NonSerialized]
        public Action onChange;

        [NonSerialized]
        internal Func<LatexExpression> getExpression;
        public LatexExpression expression => getExpression?.Invoke();

        public List<(int start, int end)> ranges => expression?.CalculateRanges(indexes) ?? new List<(int, int)>();

        // ReSharper disable Unity.PerformanceAnalysis
        public void SetGroupIndexes(params int[] values)
        {
            indexes = values.ToList();
            onChange?.Invoke();
        }

        public void SetGroupLengths(params int[] lengths)
        {
            indexes.Clear();

            lengths.Aggregate(0, (acc, x) => {
                var sum = acc + x;
                indexes.Add(sum);
                return sum;
            });

            onChange?.Invoke();
        }

#if UNITY_EDITOR
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            if (expression is null)
                return;

            LatexCharEditor.CharPreviewSize();

            var groupIndexesCopy = indexes.ToList();
            var ranges = this.ranges;
            var hasChanges = false;

            for (var i = 0; i < ranges.Count; i++) {
                var (start, end) = ranges[i];

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label($"Group {i + 1} (chars {start + 1} to {end})");
                    GUILayout.FlexibleSpace();

                    if ((i != 0) && GUILayout.Button("X")) {
                        groupIndexesCopy.RemoveAt(i - 1);
                        hasChanges = true;
                        break;
                    }
                }

                var selected = LatexCharEditor.ShowGroup(
                    expression.Slice(start, end),
                    LatexCharEditor.GetDefaultWidth()
                );

                if (selected == 0)
                    continue;

                groupIndexesCopy.Insert(i, start + selected);
                hasChanges = true;
            }

            if (!hasChanges)
                return;

            indexes = groupIndexesCopy;
            onChange?.Invoke();
        }
#endif
    }
}
