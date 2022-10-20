using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Graph2))]
public class GraphEditor : Editor
{
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();
        var graph = (Graph2)target;

        if (EditorGUI.EndChangeCheck()) {
            graph.Regenerate();
        }

        if (GUILayout.Button("Remove generated objects")) {
            graph.RemoveEditorGeneratedChildren();
        }
    }
}
