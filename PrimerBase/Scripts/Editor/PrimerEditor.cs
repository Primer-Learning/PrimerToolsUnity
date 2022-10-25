using UnityEditor;
using UnityEngine;

public class PrimerEditor<T> : Editor where T : Object
{
    readonly Color darkSkinHeaderColor = new Color32(62, 62, 62, 255);
    readonly Color lightSkinHeaderColor = new Color32(194, 194, 194, 255);

    public T component => target as T;

    protected void CustomHeader(string name) {
        var rect = EditorGUILayout.GetControlRect(false, 0f);
        rect.height = EditorGUIUtility.singleLineHeight * 1.1f;
        rect.y -= rect.height + 5;
        rect.x = 60;
        rect.xMax -= rect.x * 1.7f;

        EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? darkSkinHeaderColor : lightSkinHeaderColor);

        if (string.IsNullOrEmpty(name)) {
            name = target.ToString();
        }

        EditorGUI.LabelField(rect, name, EditorStyles.boldLabel);
    }
}
