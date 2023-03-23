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
        public static TransitionType[] Validate(this IEnumerable<TransitionType> transitions)
        {
            var array = transitions.ToArray();

            if (array.Count(x => x == TransitionType.Anchor) > 1)
                throw new Exception("Cannot have more than one anchor transition");

            return array;
        }

        public static int GetTransitionAmountFor(LatexComponent from, LatexComponent to)
        {
            return Mathf.Max(from.transform.childCount, to.transform.childCount);
        }

        public static TransitionType[] Validate(this IEnumerable<TransitionType> transitions, LatexComponent from,
            LatexComponent to)
        {
            var array = transitions.Validate();

            // If either component is processing, we can't validate the transition
            if (from.isProcessing || to.isProcessing)
                return array;

            var add = array.Count(x => x == TransitionType.Add);
            var remove = array.Count(x => x == TransitionType.Remove);
            var common = array.Length - add - remove;

            if (from.transform.childCount < common + remove) {
                if (from.groupsCount == common + remove)
                    Debug.LogWarning("LatexComponent didn't have time to update groups, the transition may fail");
                else
                    throw new Exception("Source LatexComponent doesn't have enough groups to transition");
            }

            if (to.transform.childCount < common + add) {
                if (to.groupsCount == common + add)
                    Debug.LogWarning("LatexComponent didn't have time to update groups, the transition may fail");
                else
                    throw new Exception("Target LatexComponent doesn't have enough groups to transition");
            }

            return array;
        }
    }
}
