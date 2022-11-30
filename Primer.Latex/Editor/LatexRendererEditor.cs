using System;
using System.Collections.Generic;
using Primer.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexRenderer))]
    public class LatexRendererEditor : PrimerEditor<LatexRenderer>
    {
        /// <summary>The last values for latex and headers seen on the serializedObject.</summary>
        private LatexRenderConfig lastSeenConfig;
        /// <summary>The last values sent to the LatexRenderer to execute.</summary>
        private LatexRenderConfig executedConfig;

        /// <summary>
        ///     Used instead of the actual serialized object for new latex and headers values.
        ///     So we can make sure they're only actually changed when a build finishes.
        /// </summary>
        private SerializedObject bufferObject => bufferCache ??= new SerializedObject(target);

        private SerializedObject bufferCache;

        /// <summary>Will be true if we are editing a preset.</summary>
        /// <remarks>
        ///     This condition was found through exploration... There is no documented way to determine
        ///     whether we're currently editing a preset. There's likely to be other cases where this is true
        ///     that we'll want to figure out how to exclude. But we'll handle those as needed.
        /// </remarks>
        private bool isTargetAPreset => component.gameObject.scene.handle == 0;


        #region Rendering request queue
        private static readonly Dictionary<LatexRenderer, LatexRenderConfig> pendingSetLatex = new();

        /// <summary>
        ///     Pends an attempt to set the latex and headers for a given LatexRenderer. Whenever an
        ///     editor for that LatexRenderer is rendered it will attempt to build the latex and headers given,
        ///     as if the user had entered the values themselves.
        /// </summary>
        internal static void PendRenderingRequest(LatexRenderer latexRenderer, LatexRenderConfig config) =>
            pendingSetLatex.Add(latexRenderer, config);
        #endregion


        private LatexRenderConfig GetConfig(SerializedObject obj) => new(
            obj.FindProperty(nameof(component.latex)).stringValue,
            obj.FindProperty(nameof(component.headers)).GetStringArrayValue()
        );
        private void SetConfig(SerializedObject obj, LatexRenderConfig config)
        {
            obj.FindProperty(nameof(component.latex)).stringValue = config.Latex;
            obj.FindProperty(nameof(component.headers)).SetStringArrayValue(config.Headers);
        }


        public override bool RequiresConstantRepaint() => true;
        private void OnEnable() => lastSeenConfig = GetConfig(serializedObject);


        public override void OnInspectorGUI()
        {
            if (HandleIfPreset()) {
                base.OnInspectorGUI();
                return;
            }

            UpdateBufferObject();
            ProcessPendingTasks();

            GetStatusBox().Render();

            RenderOpenBuildDirButton();
            RenderCancelButton();

            Space();
            RenderBufferedLatexAndHeadersFields();
            Space();
            PropertyField(nameof(component.material));
            Space();
            PropertyField("gizmos");
            Space();

            if (RenderReleaseSvgPartsButton())
                return;

            if (!component.isRunning && NeedsExecution())
                ExecuteRender();

            serializedObject.ApplyModifiedProperties();
            lastSeenConfig = GetConfig(serializedObject);
        }


        #region OnInspectorGUI parts
        public static readonly GUILayoutOption latexInputHeight =
            GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 6);

        public static readonly EditorHelpBox targetIsPresetWarning = EditorHelpBox.Warning(
            "You are editing a preset and the LaTeX will not be built until " +
            "you apply the preset to an actual LatexRenderer component."
        );

        private bool HandleIfPreset()
        {
            if (!isTargetAPreset) return false;

            component.characters = Array.Empty<LatexChar>();
            serializedObject.Update();
            targetIsPresetWarning.Render();
            return true;
        }

        /// <summary>Updates bufferObject if it was changed outside of this class (ex: by an undo operation).</summary>
        private void UpdateBufferObject()
        {
            serializedObject.Update();

            var currentConfig = GetConfig(serializedObject);
            if (currentConfig == lastSeenConfig && currentConfig == executedConfig) return;

            bufferObject.Update();
            bufferObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private EditorHelpBox GetStatusBox()
        {
            if (component.isCancelled && component.isRunning) {
                return EditorHelpBox.Warning("Cancelling...");
            }

            if (component.isRunning)
                return EditorHelpBox.Warning("Rendering LaTeX...");

            if (component.renderError is not null)
                return EditorHelpBox.Error(component.renderError.Message);

            return EditorHelpBox.Info("Ok");
        }

        private void ProcessPendingTasks()
        {
            if (!pendingSetLatex.TryGetValue(component, out var pending))
                return;

            SetConfig(bufferObject, pending);
            pendingSetLatex.Remove(component);
        }

        private void RenderOpenBuildDirButton()
        {
            if (GUILayout.Button("Open Build Directory"))
                component.OpenBuildDir();
        }

        private void RenderCancelButton()
        {
            EditorGUI.BeginDisabledGroup(!component.isRunning);
            if (GUILayout.Button("Cancel Rendering Task"))
                component.CancelRender();
            EditorGUI.EndDisabledGroup();
        }

        private void RenderBufferedLatexAndHeadersFields()
        {
            var latex = bufferObject.FindProperty(nameof(component.latex)).stringValue;

            EditorGUILayout.LabelField("Latex");
            EditorGUILayout.TextArea(latex, latexInputHeight);
            Space();
            EditorGUILayout.PropertyField(bufferObject.FindProperty(nameof(component.headers)));
        }

        private bool RenderReleaseSvgPartsButton()
        {
            if (!GUILayout.Button("Release SVG Parts")) return false;

            Undo.SetCurrentGroupName("Release SVG Parts");
            var releasedRenderer = component.ReleaseSvgParts();
            Undo.RegisterCreatedObjectUndo(releasedRenderer, "Released SVG parts");
            Undo.DestroyObjectImmediate(component);
            return true;
        }

        private bool NeedsExecution()
        {
            var bufferedConfig = GetConfig(bufferObject);
            var lastAppliedConfig = component.Config;
            var isBufferDifferentThanLastExecution = bufferedConfig != executedConfig;

            if (bufferedConfig != lastAppliedConfig && isBufferDifferentThanLastExecution)
                return true;

            if (component.isValid)
                return false;

            return isBufferDifferentThanLastExecution || component.renderError is not null;
        }

        private async void ExecuteRender()
        {
            executedConfig = GetConfig(bufferObject);
            await component.Render(executedConfig);
            SetConfig(serializedObject, executedConfig);
            serializedObject.ApplyModifiedProperties();
        }
        #endregion
    }
}
