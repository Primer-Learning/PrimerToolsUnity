using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEditor;
using UnityEngine;

namespace Primer.Latex.Editor
{
    [UsedImplicitly]
    internal class LatexTransitionGroupDrawer : OdinAttributeDrawer<LatexTransitionGroupAttribute, List<TransitionType>>
    {
        private ValueResolver<LatexComponent> fromResolver;
        private ValueResolver<LatexComponent> toResolver;
        private Rect rect;

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
            var value = ValueEntry.SmartValue;

            if (from == null || to == null || value == null)
                return;

            try {
                value.Validate(from, to);
            }
            catch {
                // Reset to a sensible value
                ValueEntry.SmartValue =
                    (new TransitionType[TransitionTypeExtensions.GetTransitionAmountFor(from, to)]).ToList();
            }

            PrimerLogger.Log(new { from, to, value });

            from.ConnectParts();
            to.ConnectParts();

            var fromCursor = 0;
            var toCursor = 0;
            var fromGroups = from.GetGroups().ToList();
            var toGroups = to.GetGroups().ToList();

            rect = EditorGUILayout.GetControlRect();
            var transitions = value.ToList(); // .Validate(from, to);
            var newValues = new List<TransitionType>();

            foreach (var transition in transitions) {
                var newValue = EditorGUILayout.EnumPopup("", transition);
                newValues.Add(newValue is TransitionType kind ? kind : TransitionType.Transition);

                var fromGroup = fromGroups.ElementAtOrDefault(fromCursor);
                var toGroup = toGroups.ElementAtOrDefault(toCursor);

                if (transition == TransitionType.Add)
                    LatexCharEditor.ShowGroup(toGroup);
                else
                    fromCursor++;

                if (transition == TransitionType.Remove)
                    LatexCharEditor.ShowGroup(fromGroup);
                else
                    toCursor++;

                if (transition is TransitionType.Add or TransitionType.Remove)
                    continue;

                if (fromGroup is not null)
                    LatexCharEditor.ShowGroup(fromGroup);

                if (toGroup is not null)
                    LatexCharEditor.ShowGroup(toGroup);
            }

            if (!newValues.SequenceEqual(transitions))
                ValueEntry.SmartValue = newValues;
        }
    }
}
