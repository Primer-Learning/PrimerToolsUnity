using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexTransition))]
    public class LatexTransitionEditor : OdinEditor
    {
        public LatexTransition transition => (LatexTransition)target;

        private float t = 0.0f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(16);
            GUILayout.Label("Preview");

            var newT = EditorGUILayout.Slider(t, 0, 1);
            var evaluate = newT != t;

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("0")) {
                    newT = 0;
                    evaluate = true;
                }

                if (GUILayout.Button("Clear")) {
                    transition.Deactivate();
                    evaluate = false;
                }

                if (GUILayout.Button("1")) {
                    newT = 1;
                    evaluate = true;
                }
            }

            if (evaluate) {
                transition.ToTween().Evaluate(newT);
                t = newT;
            }
        }
    }
}
