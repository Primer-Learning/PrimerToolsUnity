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
                GroupTransitionType.Transition => $"{nameof(GroupTransitionType)}.{nameof(GroupTransitionType.Transition)}",
                GroupTransitionType.Anchor => $"{nameof(GroupTransitionType)}.{nameof(GroupTransitionType.Anchor)}",
                GroupTransitionType.Add => $"{nameof(GroupTransitionType)}.{nameof(GroupTransitionType.Add)}",
                GroupTransitionType.Remove => $"{nameof(GroupTransitionType)}.{nameof(GroupTransitionType.Remove)}",
                GroupTransitionType.Replace => $"{nameof(GroupTransitionType)}.{nameof(GroupTransitionType.Replace)}",
                _ => throw new ArgumentOutOfRangeException(nameof(transition), transition, null)
            };
        }

        public static IEnumerator<(int, int?, int?)> GetGroupIndexes(this IEnumerable<GroupTransitionType> transitions)
        {
            var list = transitions.ToList();
            var fromCursor = 0;
            var toCursor = 0;

            for (var i = 0; i < list.Count; i++) {
                var transition = list[i];

                yield return (
                    i,
                    transition is GroupTransitionType.Add ? null : fromCursor++,
                    transition is GroupTransitionType.Remove ? null : toCursor++
                );
            }
        }

        public static void AdjustLength(this List<GroupTransitionType> transitions, LatexComponent from, LatexComponent to)
            => AdjustLength(transitions, from.groupsCount, to.groupsCount);

        public static void AdjustLength(this List<GroupTransitionType> transitions, int fromGroupsCount, int toGroupsCount)
        {
            var fromCursor = 0;
            var toCursor = 0;

            foreach (var transition in transitions) {
                if (transition is not GroupTransitionType.Add)
                    fromCursor++;

                if (transition is not GroupTransitionType.Remove)
                    toCursor++;
            }

            while (fromCursor < fromGroupsCount && toCursor < toGroupsCount) {
                transitions.Add(GroupTransitionType.Transition);
                fromCursor++;
                toCursor++;
            }

            while (fromCursor < fromGroupsCount) {
                transitions.Add(GroupTransitionType.Remove);
                fromCursor++;
            }

            while (toCursor < toGroupsCount) {
                transitions.Add(GroupTransitionType.Add);
                toCursor++;
            }

            while (transitions.Count is not 0 && fromCursor > fromGroupsCount) {
                var index = transitions.LastIndexOf(GroupTransitionType.Remove);
                transitions.RemoveAt(index == -1 ? transitions.Count - 1 : index);
                fromCursor--;
            }

            while (transitions.Count is not 0 && toCursor > toGroupsCount) {
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
