using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PrimerEditor<T> : Editor where T : class
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

    protected void DerivedTypeSelectorWithProps<TParentClass>(string propertyName) {
        var derivedTypeProperties = DerivedTypeSelector<TParentClass>(propertyName);

        foreach (var childProperty in GetImmediateChildren(derivedTypeProperties))
            EditorGUILayout.PropertyField(childProperty);
    }

    protected SerializedProperty DerivedTypeSelector<TParentClass>(string propertyName, bool allowEmpty = true) {
        var choices = TypeCache.GetTypesDerivedFrom<TParentClass>().ToList();
        if (allowEmpty) choices.Insert(0, null);

        var property = serializedObject.FindProperty(propertyName);
        var selectedIndex = choices.IndexOf(property.managedReferenceValue?.GetType());
        if (selectedIndex == -1) selectedIndex = 0;

        var newIndex = EditorGUILayout.Popup(
            new GUIContent(property.displayName, property.tooltip),
            selectedIndex,
            choices.Select(i => i?.Name ?? "None").ToArray()
        );

        var newType = choices[newIndex];

        if (newIndex != selectedIndex) {
            property.managedReferenceValue = newType is null
                ? null
                : Activator.CreateInstance(newType);
        }

        return property;
    }

    IEnumerable<SerializedProperty> GetImmediateChildren(SerializedProperty parent) {
        var dots = parent.propertyPath.Count(c => c == '.');

        foreach (SerializedProperty inner in parent) {
            var isDirectChild = inner.propertyPath.Count(c => c == '.') == dots + 1;
            if (isDirectChild)
                yield return inner;
        }
    }
}
