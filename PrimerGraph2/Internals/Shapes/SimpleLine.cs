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


        public ILine Crop(float newLength) {
            if (newLength.IsInteger() && Length == (int)newLength) {
                return this;
            }

            if (Length < newLength) {
                throw new Exception("Crop size is bigger than grid area. Do you want IGrid.Resize()?");
            }

            var finalLength = Mathf.CeilToInt(newLength);
            var lastIndex = finalLength - 1;

            if (lastIndex == 0) {
                return zero;
            }

            var points = Points;
            var copy = new Vector3[finalLength];

            // Copy unchanged points
            for (var i = 0; i < finalLength; i++) {
                copy[i] = points[i];
            }

            try {
                var a = points[lastIndex - 1];
                var b = points[lastIndex];
                var t = newLength.GetDecimals();

                copy[lastIndex] = Vector3.Lerp(a, b, t);
            }
            catch (Exception ex) {
                Debug.Log($"POTATO {lastIndex} {points.Length}");
            }

            return new SimpleLine(copy);
        }
    }
}
