using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexComponent))]
    public class LatexComponentEditor : OdinEditor
    {
        private LatexComponent component => target as LatexComponent;

        // This ensures the InfoBox gets update as the program processes the LaTeX
        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            if (component.gameObject.IsPreset()) {
                ShowPresetWarning();
                return;
            }

            base.OnInspectorGUI();
        }

        private void ShowPresetWarning()
        {
            var message = "You are editing a preset and the LaTeX will not be built until "
                + "you apply the preset to an actual LatexRenderer component.";

            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
    }
}
