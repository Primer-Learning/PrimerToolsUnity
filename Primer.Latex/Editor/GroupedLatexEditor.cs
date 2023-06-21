using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(GroupedLatex))]
    public class GroupedLatexEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            var groups = (GroupedLatex)target;
            var width = LatexCharEditor.GetDefaultWidth();

            LatexCharEditor.CharPreviewSize();

            foreach (var (index, (start, end)) in groups.CalculateRanges().WithIndex()) {
                var group = groups.expression.Slice(start, end);

                using (new EditorGUILayout.HorizontalScope()) {
                    GUILayout.Label($"Group {index + 1}");

                    if (index != 0 && GUILayout.Button("X", GUILayout.Width(20))) {
                        groups.groupIndexes.RemoveAt(index - 1);
                        break;
                    }
                }

                var clicked = LatexCharEditor.ShowGroup(group, width);

                if (clicked is not 0) {
                    groups.groupIndexes.Insert(index, start + clicked);
                }
            }
        }
    }
}
