using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph.Editor
{
    [CustomTimelineEditor(typeof(PlotEquationClip))]
    [UsedImplicitly]
    public class PlotEquationClipGui : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip) {
            var plotter = (PlotEquationClip)clip.asset;
            var equation = plotter.template?.equation;

            if (equation is null) {
                clip.displayName = "Equation";
            }
            else {
                var equationName = equation.GetType().Name.Replace("Equation", "");
                clip.displayName = $"Equation ({equationName})";
            }

            base.OnClipChanged(clip);
        }
    }
}
