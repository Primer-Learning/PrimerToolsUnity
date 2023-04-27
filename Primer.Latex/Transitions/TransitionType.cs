using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    public enum TransitionType
    {
        Transition,
        Anchor,
        Add,
        Remove,
        Replace,
    }

    public static class TransitionTypeExtensions
    {
        public static string ToCode(this TransitionType transition)
        {
            return transition switch {
                TransitionType.Transition => "TransitionType.Transition",
                TransitionType.Anchor => "TransitionType.Anchor",
                TransitionType.Add => "TransitionType.Add",
                TransitionType.Remove => "TransitionType.Remove",
                TransitionType.Replace => "TransitionType.Replace",
                _ => throw new ArgumentOutOfRangeException(nameof(transition), transition, null)
            };
        }

        public static void AdjustLength(this List<TransitionType> transitions, LatexComponent from, LatexComponent to)
        {
            var fromCursor = 0;
            var toCursor = 0;
            var fromGroups = from.groupsCount;
            var toGroups = to.groupsCount;

            foreach (var transition in transitions) {
                if (transition is not TransitionType.Add)
                    fromCursor++;

                if (transition is not TransitionType.Remove)
                    toCursor++;
            }

            while (fromCursor < fromGroups) {
                transitions.Add(TransitionType.Remove);
                fromCursor++;
            }

            while (fromCursor > fromGroups) {
                var index = transitions.LastIndexOf(TransitionType.Remove);
                transitions.RemoveAt(index == -1 ? transitions.Count - 1 : index);
                fromCursor--;
            }

            while (toCursor < toGroups) {
                transitions.Add(TransitionType.Add);
                toCursor++;
            }

            while (toCursor > toGroups) {
                var index = transitions.LastIndexOf(TransitionType.Add);
                transitions.RemoveAt(index == -1 ? transitions.Count - 1 : index);
                toCursor--;
            }
        }

        public static List<TransitionType> Validate(this IEnumerable<TransitionType> transitions)
        {
            var array = new List<TransitionType>(transitions);

            if (array.Count(x => x == TransitionType.Anchor) > 1)
                throw new Exception("Cannot have more than one anchor transition");

            return array;
        }

        public static List<TransitionType> Validate(this IEnumerable<TransitionType> transitions, LatexComponent from,
            LatexComponent to)
        {
            var list = transitions.Validate();

            // If either component is processing, we can't validate the transition
            if (from.isProcessing || to.isProcessing)
                return list;

            AdjustLength(list, from, to);
            return list;
        }
    }
}
