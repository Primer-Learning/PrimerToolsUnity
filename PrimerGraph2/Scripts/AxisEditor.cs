using UnityEditor;
[CustomEditor(typeof(Axis2))]
public class AxisEditor : Editor
{
    public override void OnInspectorGUI() {
        var axis = (Axis2)target;

        EditorGUI.BeginChangeCheck();

        var container = axis.container?.gameObject;

        if (container) {
            EditorGUILayout.LabelField(container.name, EditorStyles.boldLabel);
            EditorGUILayout.Space();
        }

        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck()) {
            axis.Regenerate();
        }
    }
}
