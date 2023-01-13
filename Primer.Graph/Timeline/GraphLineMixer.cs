using System.Collections.Generic;
using Primer.Timeline;
using Shapes;

namespace Primer.Graph
{
    // TODO: Replace CollectedMixer with PrimerMixer
    // Look at LatexMixer for an example
    public class GraphLineMixer : CollectedMixer<Polyline, ILine>
    {
        private List<PolylinePoint> originalPoints;

        protected override void Start(Polyline line)
        {
            originalPoints = line.points;

            originalValue = originalPoints.Count == 0
                ? null
                : new SimpleLine(originalPoints);
        }

        protected override void Stop(Polyline line)
        {
            if (!line)
                return;

            line.points = originalPoints;
            line.meshOutOfDate = true;
        }

        protected override ILine ProcessPlayable(PrimerPlayable behaviour) =>
            behaviour is ILineBehaviour { Points: {} } lineBehaviour
                ? lineBehaviour.Points
                : null;

        protected override void Apply(Polyline line, ILine input)
        {
            line.points = SimpleLine.ToPolyline(input);
            line.meshOutOfDate = true;
        }

        protected override ILine SingleInput(ILine input, float weight, bool isReverse) =>
            input.SmoothCut(input.Segments * weight, isReverse);

        protected override ILine Mix(List<float> weights, List<ILine> inputs)
        {
            var lines = ILine.Resize(inputs.ToArray());
            var result = lines[0];

            for (var i = 1; i < lines.Length; i++) {
                result = ILine.Lerp(result, lines[i], weights[i]);
            }

            return result;
        }
    }
}
