using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    [CustomTimelineEditor(typeof(AdapterClip))]
    [UsedImplicitly]
    public class AdapterClipGUI : ClipEditor
    {
        static ScrubbableAdapter GetAdapter(TimelineClip clip) => ((AdapterClip)clip.asset).adapter;

        public override void OnClipChanged(TimelineClip clip) {
            if (GetAdapter(clip) is {} adapter)
                clip.displayName = adapter.GetType().Name;
            else
                clip.displayName = "(Choose an adapter)";

            base.OnClipChanged(clip);
        }

        public override ClipDrawOptions GetClipOptions(TimelineClip clip) {
            var options = base.GetClipOptions(clip);

            if (GetAdapter(clip) is {} adapter && adapter.errors.Count > 0) {
                options.errorText = "Errors are preventing the clip from playing. See logs.";
            }

            return options;
        }
    }
}
