using System;
using System.Collections.Generic;
using System.Linq;
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
            // var stage = stages[selectedTab];
            var mutable = new MutableStage(prev, stage);

            RenderTransition(mutable);

            var newStage = mutable.ToStage(stage);

            if (newStage != stage)
                overrides[selectedTab] = newStage;

            if (!IsSame(stages[selectedTab], newStage)) {
                EditorGUILayout.HelpBox(
                    "Changes are not saved, used button below to copy the source code",
                    MessageType.Warning
                );
            }

            if (GUILayout.Button("Copy code"))
                CopyCode(prev, newStage);
        }

        private LatexSequence GetSequence()
        {
            if (target is not LatexSequence sequence)
                return null;

            sequence.EnsureIsInitialized();
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

            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField($"Transition {selectedTab + 1}");
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

        private static void RenderTransition(MutableStage mutable)
        {
            var width = LatexCharEditor.GetDefaultWidth();
            var forceAnchor = -1;

            foreach (var (i, transition, fromRange, toRange) in mutable) {
                EditorGUILayout.Space(4);

                using (new GUILayout.HorizontalScope()) {
                    var newValue = EditorGUILayout.EnumPopup("", transition);
                    var newTransition = newValue is GroupTransitionType kind ? kind : transition;

                    if (newTransition is GroupTransitionType.Anchor && transition is not GroupTransitionType.Anchor)
                        forceAnchor = i;

                    if (newTransition != transition)
                        mutable.SetTransitionType(i, newTransition);

                    if (i != 0 && GUILayout.Button("X", GUILayout.Width(20))) {
                        mutable.RemoveGroup(i);
                        break;
                    }
                }

                if (fromRange.HasValue) {
                    var clicked = LatexCharEditor.ShowGroup(mutable.GetGroupBefore(fromRange.Value), width);

                    if (clicked is not 0)
                        mutable.AddGroupBefore(i, fromRange.Value.start + clicked);
                }

                if (fromRange.HasValue && toRange.HasValue)
                    EditorGUILayout.Space(4);

                if (toRange.HasValue) {
                    var clicked = LatexCharEditor.ShowGroup(mutable.GetGroupAfter(toRange.Value), width);

                    if (clicked is not 0)
                        mutable.AddGroupAfter(i, toRange.Value.start + clicked);
                }
            }

            if (forceAnchor is -1)
                return;

            foreach (var (i, transition, _, _) in mutable) {
                if (transition is GroupTransitionType.Anchor && i != forceAnchor)
                    mutable.SetTransitionType(i, GroupTransitionType.Transition);
            }
        }

        private static bool IsSame(Stage left, Stage right)
        {
            return left == right || (
                left.latex == right.latex &&
                left.transition.SequenceEqual(right.transition) &&
                left.groupIndexesBefore.SequenceEqual(right.groupIndexesBefore) &&
                left.groupIndexesAfter.SequenceEqual(right.groupIndexesAfter)
            );
        }

        private static void CopyCode(LatexComponent prev, Stage stage)
        {
            var (latex, transition, groupIndexesBefore, groupIndexesAfter) = stage;
            var before = groupIndexesBefore ?? prev.groupIndexes.ToArray();
            var after = groupIndexesAfter ?? latex.groupIndexes.ToArray();

            var parameters = new List<string> {
                before.Length is 0 ? "Array.Empty<int>()" : $"new[] {{ {string.Join(", ", before)} }}",
                $"Latex(@\"{latex.latex}\")",
                after.Length is 0 ? "Array.Empty<int>()" : $"new[] {{ {string.Join(", ", after)} }}",
            };

            parameters.AddRange(transition.Select(x => x.ToCode()));
            GUIUtility.systemCopyBuffer = $"yield return Transition(\n    {string.Join(",\n    ", parameters)}\n);";
        }
    }
}
