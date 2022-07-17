using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LatexRenderer;
using UnityEngine;

namespace UnityEditor.LatexRenderer
{
    [CustomEditor(typeof(global::LatexRenderer.LatexRenderer))]
    public class LatexRendererEditor : Editor
    {
        private static readonly
            Dictionary<global::LatexRenderer.LatexRenderer, (string latex, List<string> headers )>
            _pendingSetLatex = new();

        private (CancellationTokenSource cancellationSource, Task task)? _currentTask;
        private (string latex, List<string> headers) _currentTaskValues;

        /// <summary>
        ///     The last values for latex and headers MaybeUpdateStagingObject has seen on the
        ///     serializedObject.
        /// </summary>
        private (string latex, List<string> headers) _lastSeenValues;

        /// <summary>
        ///     Used instead of the actual serialized object for new latex and headers values. So we can
        ///     make sure they're only actually changed when a build finishes.
        /// </summary>
        private SerializedObject _stagingObject;

        private global::LatexRenderer.LatexRenderer LatexRenderer =>
            (global::LatexRenderer.LatexRenderer)target;

        private void OnEnable()
        {
            _stagingObject = new SerializedObject(target);
            _lastSeenValues = GetCurrentValues();
        }

        /// <summary>
        ///     Pends an attempt to set the latex and headers for a given LatexRenderer. Whenever an
        ///     editor for that LatexRenderer is rendered it will attempt to build the latex and headers given,
        ///     as if the user had entered the values themselves.
        /// </summary>
        internal static void PendSetLatex(global::LatexRenderer.LatexRenderer latexRenderer,
            string latex, List<string> headers)
        {
            _pendingSetLatex.Add(latexRenderer, (latex, headers));
        }

        private (string message, MessageType messageType) GetTaskStatusText()
        {
            if (_currentTask.HasValue)
            {
                var (cancellationSource, task) = _currentTask.Value;
                if (cancellationSource.IsCancellationRequested && !task.IsCompleted)
                    return ("Cancelling...", MessageType.Warning);
                if (!task.IsCompleted)
                    return ("Rendering LaTeX...", MessageType.Info);
                if (task.Exception is not null &&
                    (_stagingObject.FindProperty("_latex").stringValue != LatexRenderer.Latex ||
                     !GetStringArrayValue(_stagingObject.FindProperty("_headers"))
                         .SequenceEqual(LatexRenderer.Headers)))
                    return (task.Exception.Message, MessageType.Error);
            }

            return ("OK", MessageType.Info);
        }

        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        private static List<string> GetStringArrayValue(SerializedProperty array)
        {
            var result = new List<string>();
            for (var i = 0; i < array.arraySize; ++i)
                result.Add(array.GetArrayElementAtIndex(i).stringValue);

            return result;
        }

        private static void SetStringArrayValue(SerializedProperty array, List<string> value)
        {
            array.ClearArray();
            for (var i = 0; i < value.Count; ++i)
            {
                array.InsertArrayElementAtIndex(i);
                array.GetArrayElementAtIndex(i).stringValue = value[i];
            }
        }

        /// <summary>Gets the latex and headers properties of serializedObject.</summary>
        /// <remarks>See _lastSeenValues.</remarks>
        private (string latex, List<string> headers) GetCurrentValues()
        {
            return (serializedObject.FindProperty("_latex").stringValue,
                GetStringArrayValue(serializedObject.FindProperty("_headers")));
        }

        /// <summary>Updates _stagingObject if it was changed outside of this class (ex: by an undo operation).</summary>
        private void MaybeUpdateStagingObject()
        {
            var currentValues = GetCurrentValues();

            var latexChanged = _currentTaskValues.latex != currentValues.latex &&
                               _lastSeenValues.latex != currentValues.latex;
            var headersChanged =
                (_currentTaskValues.headers is null ||
                 !currentValues.headers.SequenceEqual(_currentTaskValues.headers)) &&
                !_lastSeenValues.headers.SequenceEqual(currentValues.headers);

            if (latexChanged || headersChanged)
            {
                _stagingObject.Update();
                _stagingObject.ApplyModifiedPropertiesWithoutUndo();
            }

            _lastSeenValues = currentValues;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MaybeUpdateStagingObject();

            var (message, messageType) = GetTaskStatusText();
            EditorGUILayout.HelpBox(message, messageType);

            if (GUILayout.Button("Open Build Directory"))
                Process.Start(
                    $"{LatexRenderer.GetRootBuildDirectory()}{Path.DirectorySeparatorChar}");

            if (GUILayout.Button("Cancel Rendering Task"))
                _currentTask?.cancellationSource.Cancel();

            var latexProperty = _stagingObject.FindProperty("_latex");
            var headersProperty = _stagingObject.FindProperty("_headers");
            if (_pendingSetLatex.TryGetValue(LatexRenderer, out var pending))
            {
                latexProperty.stringValue = pending.latex;
                SetStringArrayValue(headersProperty, pending.headers);
                _pendingSetLatex.Remove(LatexRenderer);
            }

            EditorGUILayout.PropertyField(latexProperty,
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 6));

            EditorGUILayout.PropertyField(headersProperty);

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("gizmos"));

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Release SVG Parts"))
            {
                Undo.SetCurrentGroupName("Release SVG Parts");

                var partNumber = 0;
                foreach (var drawSpec in LatexRenderer._renderer._drawSpecs)
                {
                    var obj = new GameObject($"SvgPart {partNumber++}");

                    obj.AddComponent<MeshFilter>().sharedMesh = drawSpec.Mesh;
                    var renderer = obj.AddComponent<MeshRenderer>().material =
                        LatexRenderer.material;

                    obj.transform.SetParent(LatexRenderer.transform, false);
                    obj.transform.localPosition = drawSpec.Position;

                    Undo.RegisterCreatedObjectUndo(obj, "");
                }

                var releasedRenderer = LatexRenderer.gameObject
                    .AddComponent<ReleasedLatexRendererContainer.ReleasedLatexRenderer>();
                releasedRenderer.SetLatex(LatexRenderer.Latex, LatexRenderer.Headers.ToList(),
                    LatexRenderer.material);
                Undo.RegisterCreatedObjectUndo(releasedRenderer, "");

                Undo.DestroyObjectImmediate(LatexRenderer);
                return;
            }

            var isTaskRunning = _currentTask.HasValue && !_currentTask.Value.task.IsCompleted;
            if (!isTaskRunning)
            {
                var stagedHeaders = GetStringArrayValue(headersProperty);

                var isStagingDifferent = latexProperty.stringValue != LatexRenderer.Latex ||
                                         !stagedHeaders.SequenceEqual(LatexRenderer.Headers);
                var isDifferentThanLastTask =
                    latexProperty.stringValue != _currentTaskValues.latex ||
                    _currentTaskValues.headers is null ||
                    !stagedHeaders.SequenceEqual(_currentTaskValues.headers);
                if (isStagingDifferent && isDifferentThanLastTask)
                {
                    _currentTask = LatexRenderer.SetLatex(latexProperty.stringValue, stagedHeaders);
                    _currentTaskValues = (latexProperty.stringValue, stagedHeaders);
                }
            }

            serializedObject.ApplyModifiedProperties();

            _lastSeenValues = GetCurrentValues();
        }
    }
}