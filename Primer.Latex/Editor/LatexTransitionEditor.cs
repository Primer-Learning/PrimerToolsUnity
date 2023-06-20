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

            if (newT != t) {
                t = newT;
                transition.Evaluate(t);
                return;
            }

            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("0"))
                    transition.SetInitialState();

                if (GUILayout.Button("Clear"))
                    transition.Deactivate();

                if (GUILayout.Button("1"))
                    transition.SetEndState();
            }

        }
    }
}
