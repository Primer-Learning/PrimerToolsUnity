using System;
using System.Linq;
using UnityEngine;

namespace Primer.Shapes
{
    public interface ILine
    {
        int resolution { get; }
        Vector3[] points { get; }

        ILine Resize(int newResolution);
        ILine Crop(int maxResolution, bool fromOrigin);
        ILine SmoothCut(float toResolution, bool fromOrigin);


        public static ILine Lerp(ILine a, ILine b, float t) {
            var size = Mathf.Max(a.resolution, b.resolution);
            var length = size + 1;
            var finalPoints = new Vector3[length];

            if (a.resolution != size) a = a.Resize(size);
            if (b.resolution != size) b = b.Resize(size);

            for (var i = 0; i < length; i++) {
                finalPoints[i] = Vector3.Lerp(a.points[i], b.points[i], t);
            }

            return new DiscreteLine(finalPoints);
        }

        /// <summary>
        ///     Use this when resizing several grids at the same time
        ///     this ensures grids don't suffer more than one transformation
        /// </summary>
        public static ILine[] Resize(params ILine[] inputs) {
            var sameSize = new ILine[inputs.Length];
            var maxSize = inputs.Select(t => t.resolution).Prepend(0).Max();

            for (var i = 0; i < inputs.Length; i++) {
                sameSize[i] = inputs[i].Resize(maxSize);
            }

            return sameSize;
        }
    }
}
