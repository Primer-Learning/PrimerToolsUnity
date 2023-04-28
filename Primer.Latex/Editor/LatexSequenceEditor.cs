using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexSequence), editorForChildClasses: true)]
    public class LatexSequenceEditor : OdinEditor
    {
        [SerializeField] private int selectedTab = 0;
        private readonly Dictionary<int, List<TransitionType>> transitionOverrides = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (target is not LatexSequence sequence)
                return;

            sequence.EnsureIsInitialized();

            var stages = sequence.GetStages();

            if (stages.Count == 0) {
                sequence.RunTrough();
                stages = sequence.GetStages();

                if (stages.Count == 0) {
                    EditorGUILayout.LabelField("No stages found.");
                    return;
                }
            }

            DrawTabs(stages);

            var (latex, transition) = stages[selectedTab];
            var prev = selectedTab > 0 ? stages[selectedTab - 1].latex : sequence.initial;
            var transitionList = transitionOverrides.ContainsKey(selectedTab)
                ? transitionOverrides[selectedTab]
                : transition.ToList();

            var newTransition = LatexTransitionGroupDrawer.RenderDrawer(prev, latex, transitionList);

            if (newTransition != transitionList)
                transitionOverrides[selectedTab] = newTransition;

            if (!newTransition.SequenceEqual(transition))
                EditorGUILayout.HelpBox("Changes are not saved, used button below to copy the source code", MessageType.Warning);

            if (GUILayout.Button("Copy code"))
                CopyCode(latex, newTransition);
        }

        private static void CopyCode(LatexComponent latex, IEnumerable<TransitionType> transition)
        {
            GUIUtility.systemCopyBuffer = @$"
yield return Transition(
    Latex(@""{latex.latex}"")
        {latex.GroupIndexesCode().Trim()},
    {string.Join(",\n    ", transition.Select(x => x.ToCode()))}
);
            ".Trim();
        }

        private void DrawTabs(List<(LatexComponent latex, TransitionType[] transition)> stages)
        {
            var options = stages.Select((x, i) => $"[{i + 1}] => [{i + 2}]").ToList();

            if (options.Count is 0)
                return;

            LatexCharEditor.CharPreviewSize();

            EditorGUILayout.LabelField($"Select a transition:");
            GUILayout.BeginHorizontal();
            var color = GUI.color;

            for (var i = 0; i < options.Count; i++) {
                if (i % 5 == 0) {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                if (i == selectedTab)
                    GUI.color = Color.green;

                // Draw the button and check if it is clicked
                if (GUILayout.Button(options[i]))
                    selectedTab = i;

                GUI.color = color;
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField($"Transition {selectedTab + 1}");
        }
    }
}
