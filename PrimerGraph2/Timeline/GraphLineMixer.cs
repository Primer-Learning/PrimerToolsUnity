using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Graph
{
    public class GraphLineMixer : PrimerPlayable<Polyline>
    {
        List<PolylinePoint> originalPoints;
        bool hasModifiedLine;

        public override void Start(Polyline line) {
            originalPoints = line.points;
        }

        public override void Stop(Polyline line) {
            if (!line) return;
            line.points = originalPoints;
            line.meshOutOfDate = true;
        }

        public override void Frame(Polyline line, Playable playable, FrameData info) {
            var count = playable.GetInputCount();

            var totalWeight = 0f;
            var linesToMix = new List<(float weight, List<PolylinePoint> points)>();

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);

                // If this conversion fails we'll just skip this input
                // ReSharper disable once SuspiciousTypeConversion.Global
                var behaviour = inputPlayable.GetBehaviour() as ILineBehaviour;

                if (behaviour?.points is null) continue;

                linesToMix.Add((weight, behaviour.points));
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
                    ManipulateOnlyLineAppearance(linesToMix);
                }
                else {
                    linesToMix.Add((1 - totalWeight, originalPoints));
                }
            }

            line.points = linesToMix.Count == 1
                ? linesToMix[0].points
                : MixLines(linesToMix);

            line.meshOutOfDate = true;
            hasModifiedLine = true;
        }

        static void ManipulateOnlyLineAppearance(List<(float weight, List<PolylinePoint> points)> linesToMix) {
            var onlyLine = linesToMix[0].points;
            var pointsToInclude = onlyLine.Count * linesToMix[0].weight;
            var lastIndex = Mathf.FloorToInt(pointsToInclude);

            if (lastIndex == onlyLine.Count - 1) return;

            var copy = new List<PolylinePoint>();

            for (var i = 0; i < lastIndex; i++) {
                copy.Add(onlyLine[i]);
            }

            var a = onlyLine[lastIndex];
            var b = onlyLine[lastIndex + 1];
            copy.Add(PolylinePoint.Lerp(a, b, pointsToInclude % 1));

            linesToMix.Clear();
            linesToMix.Add((1, copy));
        }

        static List<PolylinePoint> MixLines(IReadOnlyList<(float weight, List<PolylinePoint> points)> pointsToMix) {
            var lines = pointsToMix.Count;
            var maxPoints = 0;

            for (var i = 0; i < lines; i++) {
                var points = pointsToMix[i].points.Count;
                if (points > maxPoints) maxPoints = points;
            }

            var normalizedLines = new PolylinePoint[lines][];

            for (var i = 0; i < lines; i++) {
                var points = pointsToMix[i].points;

                if (points.Count == 0) {
                    points = new List<PolylinePoint> {new(Vector3.zero)};
                }

                normalizedLines[i] = points.Count == maxPoints
                    ? points.ToArray()
                    : NormalizeLine(points, maxPoints);
            }

            var finalPoints = new List<PolylinePoint>();

            for (var i = 0; i < maxPoints; i++) {
                var point = pointsToMix[0].points[i];

                for (var j = 1; j < lines; j++) {
                    point = PolylinePoint.Lerp(point, normalizedLines[j][i], pointsToMix[j].weight);
                }

                finalPoints.Add(point);
            }

            return finalPoints;
        }

        static PolylinePoint[] NormalizeLine(IReadOnlyList<PolylinePoint> points, int expectedLength) {
            var currentLength = points.Count - 1;
            var result = new PolylinePoint[expectedLength];

            for (var i = 0; i < expectedLength; i++) {
                var currentIndex = (float)i / expectedLength * currentLength;
                var t = currentIndex % 1f;

                if (t == 0) {
                    result[i] = points[(int)currentIndex];
                    continue;
                }

                var a = points[Mathf.FloorToInt(currentIndex)];
                var b = points[Mathf.CeilToInt(currentIndex)];
                result[i] = PolylinePoint.Lerp(a, b, t);
            }

            return result;
        }
    }
}
