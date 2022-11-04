using Primer.Editor;
using Primer.Graph;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Curve))]
public class CurvePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        DerivedTypeSelector.CreateWithProps<Curve>(property, false);
    }
}
