using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Shapes
{
    public readonly struct DiscreteLine : ILine
    {
        public static ILine zero = new DiscreteLine(new[] { Vector3.zero });

        public int numSegments => points.Length - 1;
        public Vector3[] points { get; }


        public DiscreteLine(int length)
        {
            points = new Vector3[length];
        }

        public DiscreteLine(IEnumerable<Vector3> points)
        {
            this.points = points.ToArray();
        }

        public DiscreteLine(IEnumerable<float> points)
        {
            this.points = points.Select((y, i) => new Vector3(i, y)).ToArray();
        }

        public DiscreteLine Append(params Vector3[] data)
        {
            var newPoints = new Vector3[points.Length + data.Length];
            Array.Copy(points, newPoints, points.Length);
            Array.Copy(data, 0, newPoints, points.Length, data.Length);
            return new DiscreteLine(newPoints);
        }

        public DiscreteLine Append(params float[] data)
        {
            var newPoints = new Vector3[points.Length + data.Length];
            Array.Copy(points, newPoints, points.Length);

            for (var i = 0; i < data.Length; i++)
                newPoints[points.Length + i] = new Vector3(points.Length + i, data[i]);

            return new DiscreteLine(newPoints);
        }

        public DiscreteLine RemoveRedundantPoints()
        {
            var newPoints = new List<Vector3> { points[0] };

            for (var i = 1; i < points.Length; i++) {
                if (points[i] != newPoints[^1])
                    newPoints.Add(points[i]);
            }

            return new DiscreteLine(newPoints);
        }

        public ILine ChangeResolution(int newNumSegments) {
            if (newNumSegments == numSegments)
                return this;

            if (numSegments == 0)
                return new DiscreteLine(newNumSegments);

            var newPoints = new Vector3[newNumSegments + 1];

            // Loop through the new points, assigning old points to them
            // If we run out of old points, just repeat the last old point
            // until we are out of slots for new points
            for (var i = 0; i <= newNumSegments; i++)
            {
                if (i >= numSegments)
                {
                    newPoints[i] = points[numSegments];
                    continue;
                } 
                newPoints[i] = points[i];
            }
            
            // This code inserts points in the middle of the line
            // That's not currently desired, but keeping it here for future reference
            // for (var i = 0; i <= newNumSegments; i++) {
            //     var currentIndex = (float)i / newNumSegments * numSegments;
            //
            //     if (currentIndex.IsInteger()) {
            //         newPoints[i] = points[(int)currentIndex];
            //         continue;
            //     }
            //
            //     var a = points[Mathf.FloorToInt(currentIndex)];
            //     var b = points[Mathf.CeilToInt(currentIndex)];
            //     var t = currentIndex.GetDecimals();
            //     newPoints[i] = Vector3.Lerp(a, b, t);
            // }

            return new DiscreteLine(newPoints);
        }

        public ILine SmoothCut(float toResolution, bool fromOrigin) {
            if (toResolution.IsInteger() && numSegments == (int)toResolution)
                return this;

            if (numSegments < toResolution)
                throw new Exception("Crop size is bigger than grid area. Do you want ILine.Resize()?");

            var finalSize = Mathf.CeilToInt(toResolution);
            if (finalSize < 1) return zero;

            var firstIndex = fromOrigin ? numSegments - finalSize : 0;
            var lastIndex = firstIndex + finalSize;

            var copy = CutToArray(finalSize + 1, fromOrigin);
            var t = toResolution.GetDecimals();

            if (fromOrigin)
                copy[0] = Vector3.Lerp(copy[0], copy[1], 1 - t);
            else
                copy[lastIndex] = Vector3.Lerp(copy[lastIndex - 1], copy[lastIndex], t);

            return new DiscreteLine(copy);
        }

        public DiscreteLine ToDiscrete()
        {
            return this;
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
