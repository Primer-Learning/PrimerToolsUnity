using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    public class PrimerEditor<T> : UnityEditor.Editor where T : class
    {
        readonly Color darkSkinHeaderColor = new Color32(62, 62, 62, 255);
        readonly Color lightSkinHeaderColor = new Color32(194, 194, 194, 255);

        protected T component => target as T;

        protected float spacingPixels = 16;
        protected void Space() => GUILayout.Space(spacingPixels);
        protected static void Space(float pixels) => GUILayout.Space(pixels);

        protected void PropertyField(string name, bool includeChildren = false) =>
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name), includeChildren);


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
