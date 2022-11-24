using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ReleasedLatexRenderer = Primer.Latex.ReleasedLatexRendererContainer.ReleasedLatexRenderer;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(ReleasedLatexRenderer))]
    public class ReleasedLatexRendererEditor : UnityEditor.Editor
    {
        bool headersVisible;
        bool unreleaseVisible;

        public ReleasedLatexRenderer ReleasedRenderer => (ReleasedLatexRenderer)target;

        public override void OnInspectorGUI() {
            var latex = serializedObject.FindProperty("latex").stringValue;
            var headers = serializedObject.FindProperty("headers").GetStringArrayValue();

            GUILayout.Label("Latex (read-only)");
            GUILayout.TextArea(latex, LatexRendererEditor.latexInputHeight);

            headersVisible = EditorGUILayout.Foldout(headersVisible, "Headers (read-only)");
            if (headersVisible) {
                var headersInputHeight = GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 30);
                GUILayout.TextArea(string.Join(Environment.NewLine, headers), headersInputHeight);
            }

            unreleaseVisible = EditorGUILayout.Foldout(unreleaseVisible, "Un-release SVG Parts");
            if (!unreleaseVisible) return;

            if (ReleasedRenderer.gameObject.GetComponent<LatexRenderer>() is null) {
                EditorGUILayout.HelpBox(
                    "Un-releasing will delete all children game objects.",
                    MessageType.Warning
                );

                if (GUILayout.Button("Un-release SVG Parts"))
                    UnreleaseSvgParts();
            }
            else {
                EditorGUILayout.HelpBox(
                    "Cannot un-release when there is an existing LatexRenderer component.",
                    MessageType.Error
                );
            }
        }

        void UnreleaseSvgParts() {
            Undo.SetCurrentGroupName("Un-release SVG Parts");

            var latexRenderer = ReleasedRenderer.gameObject.AddComponent<LatexRenderer>();
            latexRenderer.material = ReleasedRenderer.Material;
            LatexRendererEditor.PendRenderingRequest(latexRenderer, ReleasedRenderer.Config);

            Undo.RegisterCreatedObjectUndo(latexRenderer, "");

            // we create a copy of the list because we're going to remove them as we go
            var children = ReleasedRenderer.transform.Cast<Transform>().ToArray();

            foreach (var child in children) {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            Undo.DestroyObjectImmediate(ReleasedRenderer);
        }
    }
}
