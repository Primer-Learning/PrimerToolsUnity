using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [CustomTimelineEditor(typeof(SequenceClip))]
    public class SequenceClipEditor : ClipEditor
    {
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var clipOptions = base.GetClipOptions(clip);

            if (clip.asset is SequenceClip sequenceClip)
                clipOptions.highlightColor = sequenceClip.clipColor;

            return clipOptions;
        }
    }
}
