using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace UnityEditor.LatexRenderer
{
    [CustomEditor(typeof(global::LatexRenderer.LatexRenderer))]
    public class LatexRendererComponentEditor : Editor
    {
        private global::LatexRenderer.LatexRenderer LatexRenderer =>
            (global::LatexRenderer.LatexRenderer)target;

        private (string message, MessageType messageType) GetTaskStatusText()
        {
            var (isRunning, exception) = LatexRenderer.GetTaskStatus();
            if (isRunning)
                return ("Rendering LaTeX...", MessageType.Info);
            if (exception is not null)
                return (exception.Message, MessageType.Error);

            return ("OK", MessageType.Info);
        }


        public override void OnInspectorGUI()
        {
            var (message, messageType) = GetTaskStatusText();
            EditorGUILayout.HelpBox(message, messageType);

            if (GUILayout.Button("Open Build Directory"))
                Process.Start(
                    $"{LatexRenderer.GetRootBuildDirectory()}{Path.DirectorySeparatorChar}");

            base.OnInspectorGUI();

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