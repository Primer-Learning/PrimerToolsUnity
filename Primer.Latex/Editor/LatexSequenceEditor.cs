using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

using Stage = Primer.Latex.LatexSequence.Stage;

namespace Primer.Latex.Editor
{
    [CustomEditor(typeof(LatexSequence), editorForChildClasses: true)]
    public class LatexSequenceEditor : OdinEditor
    {
        private const int TABS_PER_ROW = 5;

        [SerializeField]
        private int selectedTab = 0;

        private readonly Dictionary<int, Stage> overrides = new();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var sequence = GetSequence();
            if (sequence is null) return;

            var stages = sequence.GetStages();
            if (stages is null) return;

            if (stages.Count is 0) {
                EditorGUILayout.LabelField("No stages found.");
                return;
            }

            RenderTabs(stages);

            var prev = selectedTab > 0 ? stages[selectedTab - 1].latex : sequence.initial;
            var stage = overrides.ContainsKey(selectedTab) ? overrides[selectedTab] : stages[selectedTab];

            var mutable = new MutableTransitions(
                prev.expression,
                stage.groupIndexesBefore,
                stage.latex.expression,
                stage.groupIndexesAfter,
                stage.transition
            );

            EditorGUILayout.Space(16);
            mutable.RenderEditor();
            EditorGUILayout.Space(16);

            var newStage = mutable.hasChanges
                ? CreateStageFromMutable(mutable, stage)
                : stage;

            if (!IsSame(stages[selectedTab], newStage)) {
                EditorGUILayout.HelpBox(
                    "Changes are not saved, used button below to copy the source code",
                    MessageType.Warning
                );
            }

            if (GUILayout.Button("Copy code"))
                CopyCode(newStage);
        }

        private Stage CreateStageFromMutable(MutableTransitions mutable, Stage previous)
        {
            var (fromGroups, toGroups, transitions) = mutable.GetResult();

            overrides[selectedTab] = new Stage(
                previous.latex,
                transitions,
                fromGroups.ToArray(),
                toGroups.ToArray()
            );

            return overrides[selectedTab];
        }

        private LatexSequence GetSequence()
        {
            if (target is not LatexSequence sequence)
                return null;

            sequence.EnsureIsInitialized().Forget();
            return sequence;
        }

        #region Tabs
        private void RenderTabs(IEnumerable<Stage> stages)
        {
            var options = stages
                .Select((_, i) => $"[{i + 1}] => [{i + 2}]")
                .ToList();

            // This should never be true, but just in case
            if (options.Count is 0) return;

            LatexCharEditor.CharPreviewSize();

            EditorGUILayout.LabelField($"Select a transition:");
            GUILayout.BeginHorizontal();

            for (var i = 0; i < options.Count; i++) {
                if (i % TABS_PER_ROW == 0) {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }

                RenderTab(options[i], i);
            }

            GUILayout.EndHorizontal();
        }

        private void RenderTab(string label, int i)
        {
            if (i != selectedTab) {
                if (GUILayout.Button(label))
                    selectedTab = i;

                return;
            }

            var color = GUI.color;
            GUI.color = Color.green;
            if (GUILayout.Button(label)) { /* noop */ }
            GUI.color = color;
        }
        #endregion


        private static bool IsSame(Stage left, Stage right)
        {
            return left == right || (
                left.latex == right.latex &&
                left.transition.SequenceEqual(right.transition) &&
                left.groupIndexesBefore.IsSame(right.groupIndexesBefore) &&
                left.groupIndexesAfter.IsSame(right.groupIndexesAfter)
            );
        }

        private static void CopyCode(Stage stage)
        {
            var (latex, transition, groupIndexesBefore, groupIndexesAfter) = stage;

            var parameters = new List<string> {
                groupIndexesBefore?.Length is 0 or null
                    ? "Array.Empty<int>()"
                    : $"new[] {{ {groupIndexesBefore.Join(", ")} }}",

                $"Latex(@\"{latex.latex}\")",

                groupIndexesAfter?.Length is 0 or null
                    ? "Array.Empty<int>()"
                    : $"new[] {{ {groupIndexesAfter.Join(", ")} }}",
            };

            parameters.AddRange(transition.Select(x => x.ToCode()));
            GUIUtility.systemCopyBuffer = $"yield return Transition(\n    {string.Join(",\n    ", parameters)}\n);";
        }
    }
}
