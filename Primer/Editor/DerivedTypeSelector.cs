using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    public static class DerivedTypeSelector
    {
        public static void CreateWithProps<TParentType>(SerializedObject serializedObject, string propertyName, bool allowEmpty = true) =>
            CreateWithProps<TParentType>(serializedObject.FindProperty(propertyName), allowEmpty);

        public static void CreateWithProps<TParentType>(SerializedProperty property, bool allowEmpty = true) {
            Create<TParentType>(property, allowEmpty);

            foreach (var childProperty in GetImmediateChildren(property))
                EditorGUILayout.PropertyField(childProperty);
        }

        public static void Create<TParentType>(SerializedObject serializedObject, string propertyName, bool allowEmpty = true) =>
            Create<TParentType>(serializedObject.FindProperty(propertyName), allowEmpty);

        public static void Create<TParentType>(SerializedProperty property, bool allowEmpty = true) {
            var choices = TypeCache.GetTypesDerivedFrom<TParentType>().ToList();
            if (allowEmpty) choices.Insert(0, null);

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
        }

        static IEnumerable<SerializedProperty> GetImmediateChildren(SerializedProperty parent) {
            var dots = parent.propertyPath.Count(c => c == '.');

            foreach (SerializedProperty inner in parent) {
                var isDirectChild = inner.propertyPath.Count(c => c == '.') == dots + 1;
                if (isDirectChild)
                    yield return inner;
            }
        }
    }
}
