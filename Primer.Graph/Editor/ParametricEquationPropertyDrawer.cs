using Primer.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Graph.Editor
{
    [CustomPropertyDrawer(typeof(ParametricEquation))]
    public class ParametricEquationPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            DerivedTypeSelector.CreateWithProps<ParametricEquation>(property, false);
        }
    }
}
