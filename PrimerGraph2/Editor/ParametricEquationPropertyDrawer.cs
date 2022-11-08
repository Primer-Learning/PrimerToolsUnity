using Primer.Editor;
using Primer.Graph;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ParametricEquation))]
public class ParametricEquationPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        DerivedTypeSelector.CreateWithProps<ParametricEquation>(property, false);
    }
}
