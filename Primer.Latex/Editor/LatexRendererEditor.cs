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


        #region Rendering request queue
        private static readonly Dictionary<LatexRenderer, LatexInput> pendingSetLatex = new();

        /// <summary>
        ///     Pends an attempt to set the latex and headers for a given LatexRenderer. Whenever an
        ///     editor for that LatexRenderer is rendered it will attempt to build the latex and headers given,
        ///     as if the user had entered the values themselves.
        /// </summary>
        internal static void PendRenderingRequest(LatexRenderer latexRenderer, LatexInput config) =>
            pendingSetLatex.Add(latexRenderer, config);
        #endregion


        public override bool RequiresConstantRepaint() => true;


        public override void OnInspectorGUI()
        {
            var initialConfig = component.Config;

            base.OnInspectorGUI();

            if (HandleIfPreset()) return;

            ProcessPendingTasks();

            Space();
            GetStatusBox().Render();
            Space();
            RenderOpenBuildDirButton();
            RenderCancelButton();

            if (RenderReleaseSvgPartsButton())
                return;

            var newConfig = component.Config;

            if (!initialConfig.Equals(newConfig)) {
                component.Process(newConfig).FireAndForget();
            }
        }


        #region OnInspectorGUI parts
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


        private EditorHelpBox GetStatusBox()
        {
            if (isCancelled && isRunning) {
                return EditorHelpBox.Warning("Cancelling...");
            }

            if (isRunning)
                return EditorHelpBox.Warning("Rendering LaTeX...");

            if (processor.renderError is not null)
                return EditorHelpBox.Error(processor.renderError.Message);

            return EditorHelpBox.Info("Ok");
        }

        private void ProcessPendingTasks()
        {
            if (!pendingSetLatex.TryGetValue(component, out var pending))
                return;

            component.Process(pending).FireAndForget();
            pendingSetLatex.Remove(component);
        }

        private void RenderOpenBuildDirButton()
        {
            if (GUILayout.Button("Open Build Directory"))
                processor.OpenBuildDir();
        }

        private void RenderCancelButton()
        {
            EditorGUI.BeginDisabledGroup(!isRunning);
            if (GUILayout.Button("Cancel Rendering Task"))
                processor.Cancel();
            EditorGUI.EndDisabledGroup();
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
        #endregion
    }
}
