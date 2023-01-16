using System.Collections.Generic;
using System.Linq;
using Primer.Timeline;
using Shapes;

namespace Primer.Graph
{
    public class GraphLineMixer : PrimerMixer<Polyline, ILine>
    {
        private List<PolylinePoint> originalPoints;

        protected override void Start()
        {
            originalPoints = trackTarget.points;
        }

        protected override void Stop()
        {
            if (!trackTarget)
                return;

            trackTarget.points = originalPoints;
            trackTarget.meshOutOfDate = true;
        }


        protected override IMixerCollector<ILine> CreateCollector()
        {
            return new CollectorWithDirection<PrimerPlayable, ILine>(
                behaviour =>
                    behaviour is ILineBehaviour { Points: {} } lineBehaviour
                        ? lineBehaviour.Points
                        : null
            );
        }

        protected override void Frame(IMixerCollector<ILine> genericCollector)
        {
            var collector = (CollectorWithDirection<PrimerPlayable, ILine>)genericCollector;

            var state = collector.count > 1
                ? Mix(collector.weights, collector.inputs)
                : collector.isFull
                    ? collector[0].input
                    : CutLine(collector[0].input, collector[0].weight, collector.isReverse);

            ApplyState(state);
        }

        protected static ILine CutLine(ILine input, float weight, bool isReverse) =>
            input.SmoothCut(input.Segments * weight, isReverse);

        protected static ILine Mix(IReadOnlyList<float> weights, IReadOnlyList<ILine> inputs)
        {
            var lines = ILine.Resize(inputs.ToArray());
            var result = lines[0];

            for (var i = 1; i < lines.Length; i++) {
                result = ILine.Lerp(result, lines[i], weights[i]);
            }

            return result;
        }

        protected void ApplyState(ILine input)
        {
            trackTarget.points = SimpleLine.ToPolyline(input);
            trackTarget.meshOutOfDate = true;
        }
    }
}
