using Primer.Editor;
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

        private bool internalsVisible;

        private LatexProcessor processor => component.processor;
        private bool isRunning => processor.state == LatexProcessingState.Processing;
        private bool isCancelled => processor.state == LatexProcessingState.Cancelled;


        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            GetStatusBox().Render();
            PropertyField(nameof(component.latex));

            internalsVisible = EditorGUILayout.Foldout(internalsVisible, "Details");

            if (internalsVisible) {
                PropertyField(nameof(component.material));
                PropertyField(nameof(component.gizmos));
                Space();
                PropertyField(nameof(component.headers), true);
                Space();
                PropertyField(nameof(component.onChange));
            }

            if (HandleIfPreset())
                return;

            Space();

            if (GUILayout.Button("Open Build Directory"))
                processor.OpenBuildDir();

            using (new EditorGUI.DisabledScope(!isRunning)) {
                if (GUILayout.Button("Cancel Rendering Task"))
                    processor.Cancel();
            }

            if (GUILayout.Button("Update children"))
                component.UpdateChildren();

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
            if (isCancelled && isRunning)
                return EditorHelpBox.Warning("Cancelling...");

            if (isRunning)
                return EditorHelpBox.Warning("Rendering LaTeX...");

            if (processor.renderError is not null)
                return EditorHelpBox.Error(processor.renderError.Message);

            return EditorHelpBox.Info("Ok");
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
