using Primer;
using Primer.Editor;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PrimerAnimator))]
public class PrimerAnimatorPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        DerivedTypeSelector.CreateWithProps<PrimerAnimator>(property, false);
    }
}
