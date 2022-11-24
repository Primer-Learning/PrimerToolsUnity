using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Primer.Editor;
using UnityEditor;
using UnityEngine;
using ReleasedLatexRenderer = Primer.Latex.ReleasedLatexRendererContainer.ReleasedLatexRenderer;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexRenderer))]
    public class LatexRendererEditor : UnityEditor.Editor
    {
        public static readonly GUILayoutOption latexInputHeight =
            GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 6);

        public static readonly EditorHelpBox targetIsPresetWarning = EditorHelpBox.Warning(
            "You are editing a preset and the LaTeX will not be built until " +
            "you apply the preset to an actual LatexRenderer component."
        );

        static readonly Dictionary<LatexRenderer, LatexRenderConfig> pendingSetLatex = new();

        (CancellationTokenSource cancellationSource, Task task)? currentTask;
        LatexRenderConfig currentConfig;

        /// <summary>
        ///     The last values for latex and headers MaybeUpdateStagingObject has seen on the
        ///     serializedObject.
        /// </summary>
        LatexRenderConfig lastSeenConfig;

        /// <summary>
        ///     Used instead of the actual serialized object for new latex and headers values.
        ///     So we can make sure they're only actually changed when a build finishes.
        /// </summary>
        SerializedObject stagingObject;
        SerializedObject StagingObject => stagingObject ??= new SerializedObject(target);


        LatexRenderer Renderer => (LatexRenderer)target;

        /// <summary>Gets the latex and headers properties of serializedObject.</summary>
        LatexRenderConfig CurrentConfig => new(
            serializedObject.FindProperty("latex").stringValue,
            serializedObject.FindProperty("headers")?.GetStringArrayValue()
        );

        LatexRenderConfig StagingConfig => new(
            StagingObject.FindProperty("latex").stringValue,
            StagingObject.FindProperty("headers")?.GetStringArrayValue()
        );

        /// <summary>Will be true if we are editing a preset.</summary>
        /// <remarks>
        ///     This condition was found through exploration... There is no documented way to determine
        ///     whether we're currently editing a preset. There's likely to be other cases where this is true
        ///     that we'll want to figure out how to exclude. But we'll handle those as needed.
        /// </remarks>
        bool IsTargetAPreset => Renderer.gameObject.scene.handle == 0;


        void OnEnable() => lastSeenConfig = CurrentConfig;
        public override bool RequiresConstantRepaint() => true;


        /// <summary>
        ///     Pends an attempt to set the latex and headers for a given LatexRenderer. Whenever an
        ///     editor for that LatexRenderer is rendered it will attempt to build the latex and headers given,
        ///     as if the user had entered the values themselves.
        /// </summary>
        internal static void PendRenderingRequest(LatexRenderer latexRenderer, LatexRenderConfig config) {
            pendingSetLatex.Add(latexRenderer, config);
        }


        public override void OnInspectorGUI() {
            if (IsTargetAPreset) {
                Renderer.sprites = new Sprite[] {null};
                Renderer.spritesPositions = null;
                serializedObject.Update();

                base.OnInspectorGUI();
                targetIsPresetWarning.Render();
                return;
            }

            serializedObject.Update();
            MaybeUpdateStagingObject();
            GetStatusHelpBox()?.Render();

            var isTaskRunning = currentTask?.task.IsCompleted == false;

            if (GUILayout.Button("Open Build Directory"))
                Process.Start($"{Renderer.rootBuildDirectory}{Path.DirectorySeparatorChar}");

            EditorGUI.BeginDisabledGroup(!isTaskRunning);
            if (GUILayout.Button("Cancel Rendering Task"))
                currentTask?.cancellationSource.Cancel();
            EditorGUI.EndDisabledGroup();

            var latexProperty = StagingObject.FindProperty("latex");
            var headersProperty = StagingObject.FindProperty("headers");

            if (pendingSetLatex.TryGetValue(Renderer, out var pending)) {
                latexProperty.stringValue = pending.Latex;
                headersProperty.SetStringArrayValue(pending.Headers);
                pendingSetLatex.Remove(Renderer);
            }

            EditorGUILayout.PropertyField(latexProperty, latexInputHeight);
            EditorGUILayout.PropertyField(headersProperty);
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmos"));
            EditorGUILayout.Space(10);

            if (GUILayout.Button("Release SVG Parts")) {
                ReleaseSvgParts();
                return;
            }

            if (!isTaskRunning) {
                var stagedConfig = StagingConfig;
                var needsRebuild = !Renderer.AreSpritesValid;
                var didTaskFail = currentTask?.task.IsFaulted == true;
                var isStagingDifferent = stagedConfig != Renderer.Config;
                var isDifferentThanLastTask = stagedConfig != currentConfig;

                if ((needsRebuild && (isDifferentThanLastTask || !didTaskFail)) || (isStagingDifferent && isDifferentThanLastTask)) {
                    var cancellationTokenSource = new CancellationTokenSource();
                    currentConfig = stagedConfig;
                    currentTask = (
                        cancellationTokenSource,
                        Renderer.RenderLatex(stagedConfig, cancellationTokenSource.Token)
                    );
                }
            }

            serializedObject.ApplyModifiedProperties();
            lastSeenConfig = CurrentConfig;
        }


        void ReleaseSvgParts() {
            Undo.SetCurrentGroupName("Release SVG Parts");

            var drawSpecs = Renderer.spritesRenderer.drawSpecs;

            for (var i = 0; i < drawSpecs.Length; i++) {
                var drawSpec = drawSpecs[i];
                var obj = new GameObject($"SvgPart {i}");

                obj.AddComponent<MeshFilter>().sharedMesh = drawSpec.Mesh;
                obj.AddComponent<MeshRenderer>().material = Renderer.material;

                obj.transform.SetParent(Renderer.transform, false);
                obj.transform.localPosition = drawSpec.Position;

                Undo.RegisterCreatedObjectUndo(obj, "");
            }

            var releasedRenderer = Renderer.gameObject.AddComponent<ReleasedLatexRenderer>();
            releasedRenderer.SetLatex(Renderer.Config, Renderer.material);

            Undo.RegisterCreatedObjectUndo(releasedRenderer, "Released SVG parts");
            Undo.DestroyObjectImmediate(Renderer);
        }

        EditorHelpBox GetStatusHelpBox() {
            if (!currentTask.HasValue) {
                return EditorHelpBox.Info("Ok");
            }

            var (cancellationSource, task) = currentTask.Value;

            if (cancellationSource.IsCancellationRequested) {
                if (task.IsCompleted)
                    currentTask = null;
                else
                    return EditorHelpBox.Warning("Cancelling...");
            }

            if (!task.IsCompleted)
                return EditorHelpBox.Warning("Rendering LaTeX...");

            if (task.Exception is not null && Renderer.Config != StagingConfig)
                return EditorHelpBox.Error(task.Exception.Message);

            return EditorHelpBox.Info("Ok");
        }

        /// <summary>Updates stagingObject if it was changed outside of this class (ex: by an undo operation).</summary>
        void MaybeUpdateStagingObject() {
            var currentValues = CurrentConfig;
            if (currentValues == lastSeenConfig) return;

            StagingObject.Update();
            StagingObject.ApplyModifiedPropertiesWithoutUndo();
            lastSeenConfig = currentValues;
        }
    }
}
