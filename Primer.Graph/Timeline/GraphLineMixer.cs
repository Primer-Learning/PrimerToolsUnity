using System.Collections.Generic;
using Primer.Timeline;
using Shapes;

namespace Primer.Graph
{
    public class GraphLineMixer : CollectedMixer<Polyline, ILine>
    {
        List<PolylinePoint> originalPoints;

        protected override void Start(Polyline line) {
            originalPoints = line.points;
            originalValue = originalPoints.Count == 0
                ? null
                : new SimpleLine(originalPoints);
        }

        protected override void Stop(Polyline line) {
            if (!line) return;
            line.points = originalPoints;
            line.meshOutOfDate = true;
        }

        protected override ILine ProcessPlayable(PrimerPlayable behaviour) =>
            behaviour is ILineBehaviour {Points: {}} lineBehaviour
                ? lineBehaviour.Points
                : null;

        protected override ILine SingleInput(ILine input, float weight, bool isReverse) =>
            input.Crop(input.Length * weight, isReverse);

        protected override void Apply(Polyline line, ILine input) {
            line.points = SimpleLine.ToPolyline(input);
            line.meshOutOfDate = true;
        }

        protected override ILine Mix(List<float> weights, List<ILine> inputs) {
            // ILine.Lerp is going to resize the grids
            // But we calculate max size in advance so grids
            // only suffer a single transformation

            var maxPoints = 0;
            var count = inputs.Count;

            // TODO:
            // var inputs = SimpleLine.SameSize(...inputs);

            for (var i = 0; i < count; i++) {
                var length = inputs[i].Length;
                if (length > maxPoints) maxPoints = length;
            }

            var result = inputs[0].Resize(maxPoints);

            for (var i = 1; i < count; i++) {
                result = SimpleLine.Lerp(result, inputs[i], weights[i]);
            }

            return result;
        }
    }
}
