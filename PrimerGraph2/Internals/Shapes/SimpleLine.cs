using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace Primer.Graph
{
    public readonly struct SimpleLine : ILine
    {
        public static ILine zero = new SimpleLine(new[] {Vector3.zero});

        public static ILine Lerp(ILine a, ILine b, float t) =>
            new SimpleLine(ILine.Lerp(a, b, t));

        public static List<PolylinePoint> ToPolyline(ILine line) {
            var points = line.Points;
            var length = points.Length;
            var result = new List<PolylinePoint>();

            for (var i = 0; i < length; i++) {
                result.Add(new PolylinePoint(points[i]));
            }

            return result;
        }


        public int Length => Points.Length;
        public Vector3[] Points { get; }


        public SimpleLine(int length) => Points = new Vector3[length];
        public SimpleLine(Vector3[] points) => Points = points;

        public SimpleLine(List<PolylinePoint> list) {
            var length = list.Count;
            var points = new Vector3[length];

            for (var i = 0; i < length; i++) {
                points[i] = list[i].point;
            }

            Points = points;
        }


        public ILine Resize(int newLength) {
            var currentLength = Length;

            if (newLength == currentLength) {
                return this;
            }

            if (currentLength == 0) {
                return new SimpleLine(newLength);
            }

            var points = Points;
            var result = new Vector3[newLength];

            for (var i = 0; i < newLength; i++) {
                var currentIndex = (float)i / newLength * currentLength;

                if (currentIndex.IsInteger()) {
                    result[i] = points[(int)currentIndex];
                    continue;
                }

                var a = points[Mathf.FloorToInt(currentIndex)];
                var b = points[Mathf.CeilToInt(currentIndex)];
                var t = currentIndex.GetDecimals();

                result[i] = Vector3.Lerp(a, b, t);
            }

            return new SimpleLine(result);
        }


        public ILine Crop(float newLength) => Crop(newLength, false);
        public ILine Crop(float newLength, bool fromOrigin) {
            if (newLength.IsInteger() && Length == (int)newLength) {
                return this;
            }

            if (Length < newLength) {
                throw new Exception("Crop size is bigger than grid area. Do you want IGrid.Resize()?");
            }

            var finalLength = Mathf.CeilToInt(newLength);
            if (finalLength <= 1) return zero;

            var firstIndex = fromOrigin ? Length - finalLength : 0;
            var lastIndex = firstIndex + finalLength - 1;
            var points = Points;
            var copy = new Vector3[finalLength];

            // Copy unchanged points, including the one to lerp
            for (var i = 0; i < finalLength; i++) {
                copy[i] = points[firstIndex + i];
            }

            var t = newLength.GetDecimals();

            if (fromOrigin) {
                copy[0] = Vector3.Lerp(copy[0], copy[1], 1 - t);
            }
            else {
                copy[lastIndex] = Vector3.Lerp(copy[lastIndex - 1], copy[lastIndex], t);
            }

            return new SimpleLine(copy);
        }
    }
}
