using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(PointData2))]
public class PointDataEditor : PrimerEditor<PointData2>
{
    public override void OnInspectorGUI() {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck()) {
            component.UpdateChildren();
        }
    }
}

