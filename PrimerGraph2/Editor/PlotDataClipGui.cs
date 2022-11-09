using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph.Editor
{
    [CustomTimelineEditor(typeof(PlotDataClip))]
    [UsedImplicitly]
    public class PlotDataClipGui : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip) {
            var plotter = (PlotDataClip)clip.asset;
            var points = plotter.template?.Points;

            clip.displayName = $"Data ({points?.Count ?? 0} points)";

            base.OnClipChanged(clip);
        }
    }
}
