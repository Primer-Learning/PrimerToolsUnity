using System;
using System.Linq;
using DefaultNamespace;
using Primer;
using UnityEditor;
using UnityEngine;

namespace Simulation.BasketShot.Editor
{
    [CustomPropertyDrawer(typeof(StrategyPattern), true)]
    public class StrategyClassPropertyDrawer : PropertyDrawer
    {
        const bool ALLOW_EMPTY = false;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var totalHeight = EditorGUI.GetPropertyHeight(property, label, true) + spacing;

            foreach (var child in property.GetChildProperties()) {
                totalHeight += EditorGUI.GetPropertyHeight(child, label, true) + spacing;
            }

            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            CreateDropdown(position, property);
            EditorGUI.EndProperty();

            EditorGUI.indentLevel++;
            ChildPropertyFields(position, property, label);
            EditorGUI.indentLevel--;
        }

        void CreateDropdown(Rect position, SerializedProperty property) {
            var newRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var baseType = fieldInfo.FieldType;
            var choices = TypeCache.GetTypesDerivedFrom(baseType).ToList();
            if (ALLOW_EMPTY) choices.Insert(0, null);

            if (choices.Count == 0) {
                var isBaseStrategy = baseType.BaseType == typeof(StrategyPattern);
                var message = isBaseStrategy ? $"No subtypes of {baseType.Name}" : "";
                EditorGUI.LabelField(newRect, property.displayName, message);
                return;
            }

            var selectedIndex = choices.IndexOf(property.managedReferenceValue?.GetType());
            if (selectedIndex == -1) selectedIndex = 0;

            var labels = choices.Select(x => x is null ? "None" : x.Name).ToArray();

            var newIndex = EditorGUI.Popup(newRect, property.displayName, selectedIndex, labels);
            var newType = choices[newIndex];

            if (newType == property.managedReferenceValue?.GetType()) {
                return;
            }

            property.managedReferenceValue = newType is null
                ? null
                : Activator.CreateInstance(newType);
        }

        static void ChildPropertyFields(Rect position, SerializedProperty property, GUIContent label) {
            var prevHeight = EditorGUI.GetPropertyHeight(property, label, true);

            foreach (var child in property.GetChildProperties()) {
                var newRect = new Rect(
                    position.x,
                    position.y + prevHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUI.GetPropertyHeight(child, label, true)
                );

                prevHeight += newRect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(newRect, child);
            }
        }
    }
}
