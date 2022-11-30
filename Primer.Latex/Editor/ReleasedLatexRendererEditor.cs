using System;
using System.Linq;
using Primer.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexRenderer.Released))]
    public class LatexRendererReleasedEditor : PrimerEditor<LatexRenderer.Released>
    {
        private bool unreleaseVisible;

        public override void OnInspectorGUI() {
            // EditorGUILayout.LabelField("Latex (read-only)", component.latex);
            using (new EditorGUI.DisabledScope(true)) {
                PropertyField(nameof(component.latex));
                Space();
                EditorGUILayout.LabelField("Headers");
                EditorGUILayout.TextArea(string.Join(Environment.NewLine, component.headers));
                Space();
            }

            unreleaseVisible = EditorGUILayout.Foldout(unreleaseVisible, "Un-release SVG Parts");

            if (!unreleaseVisible) return;

            if (component.gameObject.GetComponent<LatexRenderer>() is not null) {
                EditorHelpBox.Error("Cannot un-release when there is an existing LatexRenderer component.").Render();
                return;
            }

            EditorHelpBox.Warning("Un-releasing will delete all children game objects.").Render();

            if (GUILayout.Button("Un-release SVG Parts"))
                UnreleaseSvgParts();
        }

        private void UnreleaseSvgParts()
        {
            Undo.SetCurrentGroupName("Un-release SVG Parts");

            var latexRenderer = component.gameObject.AddComponent<LatexRenderer>();
            latexRenderer.material = component.material;
            LatexRendererEditor.PendRenderingRequest(latexRenderer, component.config);

            Undo.RegisterCreatedObjectUndo(latexRenderer, "");

            // we create a copy of the list because we're going to remove them as we go
            var children = component.transform.Cast<Transform>().ToArray();

            foreach (var child in children) {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            Undo.DestroyObjectImmediate(component);
        }
    }
}
