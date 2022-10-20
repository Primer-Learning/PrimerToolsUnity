using UnityEditor;
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
    }
}
