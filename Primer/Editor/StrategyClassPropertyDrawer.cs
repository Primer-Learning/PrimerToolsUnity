using System;
using System.Linq;
using Primer;
using UnityEditor;
using UnityEngine;

namespace Simulation.BasketShot.Editor
{
    [CustomPropertyDrawer(typeof(StrategyPattern), true)]
    public class StrategyClassPropertyDrawer : PropertyDrawer
    {
        const bool ALLOW_EMPTY = false;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);
            CreateDropdown(position, property);

            EditorGUI.indentLevel++;
            ChildPropertyFields(position, property, label);
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        void CreateDropdown(Rect position, SerializedProperty property) {
            var baseType = fieldInfo.FieldType;
            var choices = TypeCache.GetTypesDerivedFrom(baseType).ToList();
            if (ALLOW_EMPTY) choices.Insert(0, null);

            var selectedIndex = choices.IndexOf(property.managedReferenceValue?.GetType());
            if (selectedIndex == -1) selectedIndex = 0;

            var labels = choices.Select(x => x is null ? "None" : x.Name).ToArray();
            var newIndex = EditorGUI.Popup(position, property.displayName, selectedIndex, labels);
            var newType = choices[newIndex];

            if (newType == property.managedReferenceValue?.GetType()) {
                return;
            }

            property.managedReferenceValue = newType is null
                ? null
                : Activator.CreateInstance(newType);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            var ite = property.Copy();
            var totalHeight = EditorGUI.GetPropertyHeight(property, label, true) + EditorGUIUtility.standardVerticalSpacing;

            while (ite.NextVisible(true)) {
                totalHeight += EditorGUI.GetPropertyHeight(ite, label, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return totalHeight;
        }

        static void ChildPropertyFields(Rect position, SerializedProperty property, GUIContent label) {
            var ite = property.Copy();
            var prevHeight = EditorGUI.GetPropertyHeight(property, label, true);

            while (ite.NextVisible(true)) {
                var newRect = new Rect(
                    position.x,
                    position.y + prevHeight + EditorGUIUtility.standardVerticalSpacing,
                    position.width,
                    EditorGUI.GetPropertyHeight(ite, label, true)
                );

                prevHeight += newRect.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(newRect, ite);
            }
        }
    }
}
