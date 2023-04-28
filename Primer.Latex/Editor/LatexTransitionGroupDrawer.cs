using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [UsedImplicitly]
    internal class LatexTransitionGroupDrawer : OdinAttributeDrawer<LatexTransitionGroupAttribute, List<GroupTransitionType>>
    {
        private ValueResolver<LatexComponent> fromResolver;
        private ValueResolver<LatexComponent> toResolver;

        protected override void Initialize()
        {
            fromResolver = ValueResolver.Get<LatexComponent>(Property, Attribute.from);
            toResolver = ValueResolver.Get<LatexComponent>(Property, Attribute.to);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueResolver.DrawErrors(fromResolver, toResolver);
            EditorGUILayout.LabelField(label);
            LatexCharEditor.CharPreviewSize();

            var from = fromResolver.GetValue();
            var to = toResolver.GetValue();
            var transitions = ValueEntry.SmartValue;

            var newValues = RenderDrawer(from, to, transitions);

            if (newValues == transitions)
                return;

            if (GUILayout.Button("Copy code"))
                CopyCode(newValues);

            EditorGUILayout.Space(32);

            if (newValues.SequenceEqual(transitions))
                return;

            ValueEntry.SmartValue = newValues;
            newValues.Validate(from, to);
        }

        public static List<GroupTransitionType> RenderDrawer(LatexComponent from, LatexComponent to, List<GroupTransitionType> transitions)
        {
            if (from == null || to == null || transitions == null) {
                return transitions;
            }

            from.ConnectParts();
            to.ConnectParts();

            transitions.AdjustLength(from, to);

            var fromCursor = 0;
            var toCursor = 0;
            var fromGroups = from.GetGroups().ToList();
            var toGroups = to.GetGroups().ToList();

            var width = LatexCharEditor.GetDefaultWidth();
            var newValues = new List<GroupTransitionType>();
            var forceAnchor = -1;

            foreach (var transition in transitions) {
                EditorGUILayout.Space(4);

                var newValue = EditorGUILayout.EnumPopup("", transition);
                var newTransition = newValue is GroupTransitionType kind ? kind : transition;

                if (newTransition is GroupTransitionType.Anchor && transition is not GroupTransitionType.Anchor)
                    forceAnchor = newValues.Count;

                newValues.Add(newTransition);

                var fromGroup = fromGroups.ElementAtOrDefault(fromCursor);
                var toGroup = toGroups.ElementAtOrDefault(toCursor);

                if (transition == GroupTransitionType.Add)
                    LatexCharEditor.ShowGroup(toGroup, width);
                else
                    fromCursor++;

                if (transition == GroupTransitionType.Remove)
                    LatexCharEditor.ShowGroup(fromGroup, width);
                else
                    toCursor++;

                if (transition is GroupTransitionType.Add or GroupTransitionType.Remove)
                    continue;

                if (fromGroup is not null)
                    LatexCharEditor.ShowGroup(fromGroup, width);

                EditorGUILayout.Space(4);

                if (toGroup is not null)
                    LatexCharEditor.ShowGroup(toGroup, width);
            }

            if (forceAnchor is not -1) {
                for (var i = 0; i < newValues.Count; i++) {
                    if (newValues[i] is GroupTransitionType.Anchor && i != forceAnchor)
                        newValues[i] = GroupTransitionType.Transition;
                }
            }

            return newValues;
        }

        private static void CopyCode(IEnumerable<GroupTransitionType> values)
        {
            var list = values.Select(x => x.ToCode());
            GUIUtility.systemCopyBuffer = @"new List<TransitionType> {" + '\n' + string.Join(",\n", list) + "\n}";
        }
    }
}
