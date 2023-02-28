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
    }
}
