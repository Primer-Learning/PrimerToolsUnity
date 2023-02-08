using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    public class PrimerEditor<T> : UnityEditor.Editor where T : class
    {
        protected T component => target as T;

        protected float spacingPixels = 16;

        protected void Space() => GUILayout.Space(spacingPixels);

        protected void PropertyField(string name, bool includeChildren = false)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name), includeChildren);
        }
    }
}
