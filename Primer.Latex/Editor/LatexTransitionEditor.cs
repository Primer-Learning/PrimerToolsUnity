using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexTransition))]
    public class LatexTransitionEditor : OdinEditor
    {
        private float t = 0.0f;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var transition = (LatexTransition)target;

            var mutable = new MutableTransitions(
                transition.start.expression,
                transition.start.groupIndexes,
                transition.end.expression,
                transition.end.groupIndexes,
                transition.transitions
            );

            EditorGUILayout.Space(16);
            mutable.RenderEditor();

            if (mutable.hasChanges) {
                var (fromGroups, toGroups, transitions) = mutable.GetResult();
                transition.start.groupIndexes = fromGroups;
                transition.end.groupIndexes = toGroups;
                transition.transitions = transitions;
            }

            EditorGUILayout.Space(16);
            t = RenderPreview(transition, t);
        }

        private static float RenderPreview(LatexTransition transition, float currentT)
        {
            SirenixEditorGUI.Title("Preview", "", TextAlignment.Center, false);
            EditorGUILayout.Space(16);

            var newT = EditorGUILayout.Slider(currentT, 0, 1);
            var evaluate = newT != currentT;

            EditorGUILayout.Space(16);

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

            if (!evaluate)
                return currentT;

            transition.ToTween().Evaluate(newT);
            return newT;

        }
    }
}
