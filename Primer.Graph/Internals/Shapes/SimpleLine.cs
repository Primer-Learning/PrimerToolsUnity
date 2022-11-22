using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

namespace Primer.Graph
{
    public readonly struct SimpleLine : ILine
    {
        public static ILine zero = new SimpleLine(new[] {Vector3.zero});

        public static List<PolylinePoint> ToPolyline(ILine line) {
            var points = line.Points;
            var length = points.Length;
            var result = new List<PolylinePoint>();

            for (var i = 0; i < length; i++) {
                result.Add(new PolylinePoint(points[i]));
            }

            return result;
        }


        public int Segments => Points.Length - 1;
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


        public ILine Resize(int newSize) {
            var currentSize = Segments;

            if (newSize == currentSize) {
                return this;
            }

            if (currentSize == 0) {
                return new SimpleLine(newSize);
            }

            var points = Points;
            var result = new Vector3[newSize + 1];

            for (var i = 0; i <= newSize; i++) {
                var currentIndex = (float)i / newSize * currentSize;

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

        public ILine Crop(int newSize, bool fromOrigin) =>
            new SimpleLine(CutToArray(newSize + 1, fromOrigin));

        public ILine SmoothCut(float newSize, bool fromOrigin) {
            if (newSize.IsInteger() && Segments == (int)newSize) {
                return this;
            }

            if (Segments < newSize) {
                throw new Exception("Crop size is bigger than grid area. Do you want ILine.Resize()?");
            }

            var finalSize = Mathf.CeilToInt(newSize);
            if (finalSize < 1) return zero;

            var firstIndex = fromOrigin ? Segments - finalSize : 0;
            var lastIndex = firstIndex + finalSize;

            var copy = CutToArray(finalSize + 1, fromOrigin);
            var t = newSize.GetDecimals();

            if (fromOrigin) {
                copy[0] = Vector3.Lerp(copy[0], copy[1], 1 - t);
            }
            else {
                copy[lastIndex] = Vector3.Lerp(copy[lastIndex - 1], copy[lastIndex], t);
            }

            return new SimpleLine(copy);
        }

        Vector3[] CutToArray(int newLength, bool fromOrigin) {
            var points = Points;
            var copy = new Vector3[newLength];

            var firstIndex = fromOrigin ? points.Length - newLength : 0;

            for (var i = 0; i < newLength; i++) {
                // try {
                copy[i] = points[firstIndex + i];
                // }
                // catch (Exception ex) {
                //     Debug.Log("Oh mamma");
                // }
            }

            return copy;
        }
    }
}
