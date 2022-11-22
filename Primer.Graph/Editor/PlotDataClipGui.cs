using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph.Editor
{
    [CustomTimelineEditor(typeof(PlottedDataClip))]
    [UsedImplicitly]
    public class PlotDataClipGui : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip) {
            var plotter = (PlottedDataClip)clip.asset;
            var grid = (plotter.template as ISurfaceBehaviour)?.Grid;

            clip.displayName = $"Data ({grid?.Points.Length ?? 0} points)";

            base.OnClipChanged(clip);
        }
    }
}
