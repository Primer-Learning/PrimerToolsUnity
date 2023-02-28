using System;
using System.Collections.Generic;
using System.Linq;

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
                throw new System.Exception("Cannot have more than one anchor transition");

            return array;
        }

        public static TransitionType[] Validate(this IEnumerable<TransitionType> transitions, LatexComponent from, LatexComponent to)
        {
            var array = transitions.Validate();

            var add = array.Count(x => x == TransitionType.Add);
            var remove = array.Count(x => x == TransitionType.Remove);
            var common = array.Length - add - remove;

            if (from.transform.childCount < common + remove)
                throw new Exception("Source LatexComponent doesn't have enough groups to transition");

            if (to.transform.childCount < common + add)
                throw new Exception("Target LatexComponent doesn't have enough groups to transition");

            return array;
        }
    }
}
