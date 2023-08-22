using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public static class TimelineClipExtensions
    {
        public static bool IsLocked(this TimelineClip clip)
        {
            return clip.asset is SequenceClip { isLocked: true };
        }

        public static bool IsNameAutomated(this TimelineClip clip)
        {
            return string.IsNullOrWhiteSpace(clip.displayName)
                || clip.displayName == nameof(SequenceClip)
                || clip.displayName[0] == '[';
        }
    }
}
