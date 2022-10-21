using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(Graph2))]
public class GraphEditor : PrimerEditor<Graph2>
{
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck()) {
            component.Regenerate();
        }

        if (GUILayout.Button("Remove generated objects")) {
            component.RemoveGeneratedChildren();
        }
    }
}
