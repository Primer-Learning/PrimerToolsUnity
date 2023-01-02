using System;
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

        /// <summary>Will be true if we are editing a preset.</summary>
        /// <remarks>
        ///     This condition was found through exploration... There is no documented way to determine
        ///     whether we're currently editing a preset. There's likely to be other cases where this is true
        ///     that we'll want to figure out how to exclude. But we'll handle those as needed.
        /// </remarks>
        private bool isTargetAPreset => component.gameObject.scene.handle == 0;

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        public override void OnInspectorGUI()
        {
            var initialConfig = component.config;

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

            if (component.characters.Length > 0)
                RenderGroupDefinition();

            serializedObject.ApplyModifiedProperties();
        }


        private bool HandleIfPreset()
        {
            if (!isTargetAPreset)
                return false;

            component.characters = Array.Empty<LatexChar>();
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

            var chars = component.characters;
            var serializedGroups = serializedObject.FindProperty(nameof(component.groupIndexes));
            var groups = serializedGroups.GetIntArrayValue();
            var ranges = chars.GetRanges(groups);
            var hasChanges = false;

            for (var i = 0; i < ranges.Count; i++) {
                var (start, end) = ranges[i];

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label($"Group {i + 1} (chars {start + 1} to {end})");
                    GUILayout.FlexibleSpace();

                    if ((i != 0) && GUILayout.Button("X")) {
                        groups.RemoveAt(i - 1);
                        hasChanges = true;
                        break;
                    }
                }

                var selected = LatexCharEditor.ShowGroup(chars, start, end);

                if (selected == 0)
                    continue;

                groups.Insert(i, start + selected);
                hasChanges = true;
            }

            if (hasChanges)
                serializedGroups.SetIntArrayValue(groups);
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
