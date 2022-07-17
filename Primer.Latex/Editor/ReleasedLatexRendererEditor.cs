using System;
using System.Collections.Generic;
using System.Linq;
using LatexRenderer;
using UnityEngine;

namespace UnityEditor.LatexRenderer
{
    [CustomEditor(typeof(ReleasedLatexRendererContainer.ReleasedLatexRenderer))]
    public class ReleasedLatexRendererEditor : Editor
    {
        private bool headersVisible;
        private bool unreleaseVisible;

        public ReleasedLatexRendererContainer.ReleasedLatexRenderer ReleasedLatexRenderer =>
            (ReleasedLatexRendererContainer.ReleasedLatexRenderer)target;

        private static List<string> GetStringArrayValue(SerializedProperty array)
        {
            var result = new List<string>();
            for (var i = 0; i < array.arraySize; ++i)
                result.Add(array.GetArrayElementAtIndex(i).stringValue);

            return result;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("Latex (read-only)");
            GUILayout.TextArea(serializedObject.FindProperty("_latex").stringValue,
                GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 6));

            headersVisible = EditorGUILayout.Foldout(headersVisible, "Headers (read-only)");
            if (headersVisible)
                GUILayout.TextArea(
                    string.Join(Environment.NewLine,
                        GetStringArrayValue(serializedObject.FindProperty("_headers"))),
                    GUILayout.MinHeight(EditorGUIUtility.singleLineHeight * 30));

            unreleaseVisible = EditorGUILayout.Foldout(unreleaseVisible, "Un-release SVG Parts");
            if (unreleaseVisible)
            {
                if (ReleasedLatexRenderer.gameObject
                        .GetComponent<global::LatexRenderer.LatexRenderer>() is null)
                {
                    EditorGUILayout.HelpBox("Un-releasing will delete all children game objects.",
                        MessageType.Warning);
                    if (GUILayout.Button("Un-release SVG Parts"))
                    {
                        Undo.SetCurrentGroupName("Un-release SVG Parts");

                        var latexRenderer = ReleasedLatexRenderer.gameObject
                            .AddComponent<global::LatexRenderer.LatexRenderer>();
                        LatexRendererEditor.PendSetLatex(latexRenderer, ReleasedLatexRenderer.Latex,
                            ReleasedLatexRenderer.Headers.ToList());
                        Undo.RegisterCreatedObjectUndo(latexRenderer, "");

                        var toDelete =
                            (from Transform i in ReleasedLatexRenderer.transform select i).ToList();
                        foreach (var child in toDelete)
                            Undo.DestroyObjectImmediate(child.gameObject);

                        Undo.DestroyObjectImmediate(ReleasedLatexRenderer);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Cannot un-release when there is an existing LatexRenderer component.",
                        MessageType.Error);
                }
            }
        }
    }
}