using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEditor.LatexRenderer
{
    [CustomEditor(typeof(global::LatexRenderer.LatexRenderer))]
    public class LatexRendererComponentEditor : Editor
    {
        private (CancellationTokenSource cancellationSource, Task task)? _currentTask;

        private global::LatexRenderer.LatexRenderer LatexRenderer =>
            (global::LatexRenderer.LatexRenderer)target;

        private (string message, MessageType messageType) GetTaskStatusText()
        {
            if (_currentTask.HasValue)
            {
                var (cancellationSource, task) = _currentTask.Value;
                if (cancellationSource.IsCancellationRequested && !task.IsCompleted)
                    return ("Cancelling...", MessageType.Warning);
                if (!task.IsCompleted)
                    return ("Rendering LaTeX...", MessageType.Info);
                if (task.Exception is not null)
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var (message, messageType) = GetTaskStatusText();
            EditorGUILayout.HelpBox(message, messageType);

            if (GUILayout.Button("Open Build Directory"))
                Process.Start(
                    $"{LatexRenderer.GetRootBuildDirectory()}{Path.DirectorySeparatorChar}");

            if (GUILayout.Button("Cancel Rendering Task"))
                _currentTask?.cancellationSource.Cancel();

            var latexProperty = serializedObject.FindProperty("_latex");
            EditorGUILayout.PropertyField(latexProperty);

            var headersProperty = serializedObject.FindProperty("_headers");
            EditorGUILayout.PropertyField(headersProperty);

            var isTaskRunning = _currentTask.HasValue && !_currentTask.Value.task.IsCompleted;
            if (latexProperty.stringValue != LatexRenderer.Latex && !isTaskRunning)
                _currentTask = LatexRenderer.SetLatex(latexProperty.stringValue,
                    GetStringArrayValue(headersProperty));

            DrawPropertiesExcluding(serializedObject, "_latex", "_headers");

            serializedObject.ApplyModifiedProperties();


            // if (GUILayout.Button("Release SVG Parts"))
            // {
            //     Undo.SetCurrentGroupName("Release SVG Parts");
            //
            //     var component = (LatexRendererComponent)target;
            //
            //     foreach (var svgPart in component._svgParts)
            //     {
            //         // TODO: This isn't working as expected... See the HACK in LatexRendererComponent.Start.
            //         Undo.RecordObject(svgPart, "");
            //         svgPart.hideFlags = HideFlags.None;
            //     }
            //
            //     // If we didn't clear _svgParts, the component would destroy them when it was destroyed
            //     Undo.RecordObject(component, "");
            //     component._svgParts = new List<GameObject>();
            //     Undo.DestroyObjectImmediate(component);
            // }
        }
    }
}