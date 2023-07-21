using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Shapes
{
    public readonly struct DiscreteLine : ILine
    {
        public static ILine zero = new DiscreteLine(new[] { Vector3.zero });

        public int resolution => points.Length - 1;
        public Vector3[] points { get; }


        public DiscreteLine(int length)
        {
            // Points default to Vector3.zero
            points = new Vector3[length];
        }

        public DiscreteLine(IEnumerable<Vector3> points)
        {
            this.points = points.ToArray();
        }

        public DiscreteLine Append(Vector3 point)
        {
            var newPoints = new Vector3[points.Length + 1];
            Array.Copy(points, newPoints, points.Length);
            newPoints[^1] = point;
            return new DiscreteLine(newPoints);
        }

        public ILine Resize(int newResolution) {
            if (newResolution == resolution)
                return this;

            if (resolution == 0)
                return new DiscreteLine(newResolution);

            var result = new Vector3[newResolution + 1];

            for (var i = 0; i <= newResolution; i++) {
                var currentIndex = (float)i / newResolution * resolution;

                if (currentIndex.IsInteger()) {
                    result[i] = points[(int)currentIndex];
                    continue;
                }

                var a = points[Mathf.FloorToInt(currentIndex)];
                var b = points[Mathf.CeilToInt(currentIndex)];
                var t = currentIndex.GetDecimals();
                result[i] = Vector3.Lerp(a, b, t);
            }

            return new DiscreteLine(result);
        }

        public ILine Crop(int maxResolution, bool fromOrigin) =>
            new DiscreteLine(CutToArray(maxResolution + 1, fromOrigin));

        public ILine SmoothCut(float toResolution, bool fromOrigin) {
            if (toResolution.IsInteger() && resolution == (int)toResolution)
                return this;

            if (resolution < toResolution)
                throw new Exception("Crop size is bigger than grid area. Do you want ILine.Resize()?");

            var finalSize = Mathf.CeilToInt(toResolution);
            if (finalSize < 1) return zero;

            var firstIndex = fromOrigin ? resolution - finalSize : 0;
            var lastIndex = firstIndex + finalSize;

            var copy = CutToArray(finalSize + 1, fromOrigin);
            var t = toResolution.GetDecimals();

            if (fromOrigin)
                copy[0] = Vector3.Lerp(copy[0], copy[1], 1 - t);
            else
                copy[lastIndex] = Vector3.Lerp(copy[lastIndex - 1], copy[lastIndex], t);

            return new DiscreteLine(copy);
        }

        private Vector3[] CutToArray(int newLength, bool fromOrigin) {
            var copy = new Vector3[newLength];
            var firstIndex = fromOrigin ? points.Length - newLength : 0;

            for (var i = 0; i < newLength; i++) {
                copy[i] = points[firstIndex + i];
            }

            return copy;
        }
    }
}
