using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph.Editor
{
    [CustomTimelineEditor(typeof(PlottedEquationClip))]
    [UsedImplicitly]
    public class PlotEquationClipGui : ClipEditor
    {
        public override void OnClipChanged(TimelineClip clip) {
            var plotter = (PlottedEquationClip)clip.asset;
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
