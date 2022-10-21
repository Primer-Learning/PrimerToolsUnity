using UnityEditor;
[CustomEditor(typeof(Axis2))]
public class AxisEditor : PrimerEditor<Axis2>
{
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();

        CustomHeader(component.container?.gameObject.name);
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck()) {
            component.UpdateGeneratedChildren();
        }
    }
}
