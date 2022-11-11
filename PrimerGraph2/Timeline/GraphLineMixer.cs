using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine.Playables;

namespace Primer.Graph
{
    public class GraphLineMixer : PrimerPlayable<Polyline>
    {
        List<PolylinePoint> originalPoints;
        ILine originalLine;
        bool hasModifiedLine;

        public override void Start(Polyline line) {
            originalPoints = line.points;
            originalLine = new SimpleLine(originalPoints);
        }

        public override void Stop(Polyline line) {
            if (!line) return;
            line.points = originalPoints;
            line.meshOutOfDate = true;
        }

        public override void Frame(Polyline line, Playable playable, FrameData info) {
            var count = playable.GetInputCount();

            var totalWeight = 0f;
            var linesToMix = new List<(float weight, ILine points)>();

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);
                if (inputPlayable.GetBehaviour() is not ILineBehaviour behaviour) {
                    continue;
                }

                var points = behaviour?.Points;
                if (points is null) continue;

                linesToMix.Add((weight, new SimpleLine(points)));
                totalWeight += weight;
            }

            if (totalWeight == 0) {
                if (hasModifiedLine) {
                    hasModifiedLine = false;
                    Stop(line);
                }

                return;
            }

            if (totalWeight < 1) {
                if (linesToMix.Count == 1 && originalPoints.Count == 0) {
                    ManipulateSingleLine(linesToMix);
                }
                else {
                    linesToMix.Add((1 - totalWeight, originalLine));
                }
            }

            var result = linesToMix.Count == 1
                ? linesToMix[0].points
                : MixLines(linesToMix);

            line.points = SimpleLine.ToPolyline(result);
            line.meshOutOfDate = true;
            hasModifiedLine = true;
        }


        static void ManipulateSingleLine(IList<(float weight, ILine line)> linesToMix) {
            var (weight, onlyLine) = linesToMix[0];
            linesToMix.Clear();
            linesToMix.Add((1, onlyLine.Crop(onlyLine.Length * weight)));
        }


        static ILine MixLines(IReadOnlyList<(float weight, ILine line)> linesToMix) {
            // IGrid.Lerp is going to resize the grids
            // But we calculate max size in advance so grids
            // only suffer a single transformation

            var maxPoints = 0;

            for (var i = 0; i < linesToMix.Count; i++) {
                var length = linesToMix[i].line.Length;
                if (length > maxPoints) maxPoints = length;
            }

            var result = linesToMix[0].line.Resize(maxPoints);

            for (var i = 1; i < linesToMix.Count; i++) {
                var (weight, line) = linesToMix[i];
                result = SimpleLine.Lerp(result, line, weight);
            }

            return result;
        }
    }
}
