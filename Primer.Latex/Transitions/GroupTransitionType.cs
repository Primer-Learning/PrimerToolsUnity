using System;

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
    }
}
