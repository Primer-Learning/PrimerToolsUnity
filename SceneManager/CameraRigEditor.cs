using UnityEngine;
using UnityEditor;

// [CustomEditor(typeof(CameraRig))]
public class CameraRigsEditor : Editor 
{
    SerializedProperty distance;
    
    void OnEnable()
    {
        distance = serializedObject.FindProperty("Distance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(distance);
        serializedObject.ApplyModifiedProperties();
    }
}