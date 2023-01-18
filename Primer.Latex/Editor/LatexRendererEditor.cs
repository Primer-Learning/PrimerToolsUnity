using Primer.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexRenderer))]
    public class LatexRendererEditor : PrimerEditor<LatexRenderer>
    {
        public static readonly EditorHelpBox targetIsPresetWarning = EditorHelpBox.Warning(
            "You are editing a preset and the LaTeX will not be built until "
            + "you apply the preset to an actual LatexRenderer component."
        );

        internal bool isCancelled => component.processor.state == LatexProcessingState.Cancelled;


        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            GetStatusBox().Render();

            if (HandleIfPreset())
                return;

            base.OnInspectorGUI();

            if (!component.expression.isEmpty)
                RenderGroupDefinition();

            serializedObject.ApplyModifiedProperties();
        }


        private bool HandleIfPreset()
        {
            if (!component.gameObject.IsPreset())
                return false;

            component.expression = new LatexExpression();
            serializedObject.Update();
            targetIsPresetWarning.Render();
            return true;
        }


        private EditorHelpBox GetStatusBox()
        {
            if (isCancelled && component.isRunning)
                return EditorHelpBox.Warning("Cancelling...");

            if (component.isRunning)
                return EditorHelpBox.Warning("Rendering LaTeX...");

            var error = component.processor.renderError;

            return error is null
                ? EditorHelpBox.Info("Ok")
                : EditorHelpBox.Error(error.Message);
        }


        #region Groups controls
        private void RenderGroupDefinition()
        {
            GroupsHeader();

            var serializedGroups = serializedObject.FindProperty(nameof(component.groupIndexes));
            var groupIndexes = serializedGroups.GetIntArrayValue();
            var ranges = component.expression.CalculateRanges(groupIndexes);
            var hasChanges = false;

            for (var i = 0; i < ranges.Count; i++) {
                var (start, end) = ranges[i];

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label($"Group {i + 1} (chars {start + 1} to {end})");
                    GUILayout.FlexibleSpace();

                    if ((i != 0) && GUILayout.Button("X")) {
                        groupIndexes.RemoveAt(i - 1);
                        hasChanges = true;
                        break;
                    }
                }

                var selected = LatexCharEditor.ShowGroup(component.expression.Slice(start, end));

                if (selected == 0)
                    continue;

                groupIndexes.Insert(i, start + selected);
                hasChanges = true;
            }

            if (hasChanges)
                serializedGroups.SetIntArrayValue(groupIndexes);
        }

        private void GroupsHeader()
        {
            Space();

            using (new GUILayout.HorizontalScope()) {
                GUILayout.Label(
                    "Groups",
                    new GUIStyle {
                        fontSize = 16,
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = Color.white },
                    }
                );

                GUILayout.Space(32);
                LatexCharEditor.CharPreviewSize();
            }

            Space();
        }
        #endregion
    }
}
