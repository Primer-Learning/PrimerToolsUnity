using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    public class PrimerEditor<T> : UnityEditor.Editor where T : class
    {
        readonly Color darkSkinHeaderColor = new Color32(62, 62, 62, 255);
        readonly Color lightSkinHeaderColor = new Color32(194, 194, 194, 255);

        protected T component => target as T;

        protected static void Space() => GUILayout.Space(16);
        protected void PropertyField(string name) =>
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name), true);


        protected void CustomHeader(string title) {
            var rect = EditorGUILayout.GetControlRect(false, 0f);
            rect.height = EditorGUIUtility.singleLineHeight * 1.1f;
            rect.y -= rect.height + 5;
            rect.x = 60;
            rect.xMax -= rect.x * 1.7f;

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin ? darkSkinHeaderColor : lightSkinHeaderColor);

            if (string.IsNullOrEmpty(title)) {
                title = target.ToString();
            }

            EditorGUI.LabelField(rect, title, EditorStyles.boldLabel);
        }
    }
}
