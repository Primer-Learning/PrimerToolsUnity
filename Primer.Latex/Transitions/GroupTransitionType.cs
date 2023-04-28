using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    public enum GroupTransitionType
    {
        Transition,
        Anchor,
        Add,
        Remove,
        Replace,
    }

    public static class TransitionTypeExtensions
    {
        public static string ToCode(this GroupTransitionType transition)
        {
            return transition switch {
                GroupTransitionType.Transition => "TransitionType.Transition",
                GroupTransitionType.Anchor => "TransitionType.Anchor",
                GroupTransitionType.Add => "TransitionType.Add",
                GroupTransitionType.Remove => "TransitionType.Remove",
                GroupTransitionType.Replace => "TransitionType.Replace",
                _ => throw new ArgumentOutOfRangeException(nameof(transition), transition, null)
            };
        }

        public static void AdjustLength(this List<GroupTransitionType> transitions, LatexComponent from, LatexComponent to)
        {
            var fromCursor = 0;
            var toCursor = 0;
            var fromGroups = from.groupsCount;
            var toGroups = to.groupsCount;

            foreach (var transition in transitions) {
                if (transition is not GroupTransitionType.Add)
                    fromCursor++;

                if (transition is not GroupTransitionType.Remove)
                    toCursor++;
            }

            while (fromCursor < fromGroups && toCursor < toGroups) {
                transitions.Add(GroupTransitionType.Transition);
                fromCursor++;
                toCursor++;
            }

            while (fromCursor < fromGroups) {
                transitions.Add(GroupTransitionType.Remove);
                fromCursor++;
            }

            while (toCursor < toGroups) {
                transitions.Add(GroupTransitionType.Add);
                toCursor++;
            }

            while (fromCursor > fromGroups) {
                var index = transitions.LastIndexOf(GroupTransitionType.Remove);
                transitions.RemoveAt(index == -1 ? transitions.Count - 1 : index);
                fromCursor--;
            }

            while (toCursor > toGroups) {
                var index = transitions.LastIndexOf(GroupTransitionType.Add);
                transitions.RemoveAt(index == -1 ? transitions.Count - 1 : index);
                toCursor--;
            }
        }

        public static List<GroupTransitionType> Validate(this IEnumerable<GroupTransitionType> transitions)
        {
            var array = new List<GroupTransitionType>(transitions);

            if (array.Count(x => x == GroupTransitionType.Anchor) > 1)
                throw new Exception("Cannot have more than one anchor transition");

            return array;
        }

        public static List<GroupTransitionType> Validate(this IEnumerable<GroupTransitionType> transitions, LatexComponent from,
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
