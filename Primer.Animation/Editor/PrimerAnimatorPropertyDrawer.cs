using Primer.Animation;
using UnityEditor;
using UnityEngine;

namespace Primer.Editor
{
    [CustomPropertyDrawer(typeof(PrimerAnimator))]
    public class PrimerAnimatorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            DerivedTypeSelector.CreateWithProps<PrimerAnimator>(property, false);
        }
    }
}
