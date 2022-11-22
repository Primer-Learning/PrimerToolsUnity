using System;
using UnityEngine;

namespace Primer.Graph
{
    public interface ILine
    {
        int Segments { get; }
        Vector3[] Points { get; }

        ILine Resize(int newSize);
        ILine Crop(int newSize, bool fromOrigin);
        ILine SmoothCut(float newSize, bool fromOrigin);


        public static T Lerp<T>(T a, T b, float t) where T : ILine {
            var size = a.Segments > b.Segments ? a.Segments : b.Segments;
            var length = size + 1;
            var finalPoints = new Vector3[length];

            if (a.Segments != size) a = (T)a.Resize(size);
            if (b.Segments != size) b = (T)b.Resize(size);

            for (var i = 0; i < length; i++) {
                finalPoints[i] = Vector3.Lerp(a.Points[i], b.Points[i], t);
            }

            return (T)Activator.CreateInstance(a.GetType(), finalPoints);
        }

        /// <summary>
        ///     Use this when resizing several grids at the same time
        ///     this ensures grids don't suffer more than one transformation
        /// </summary>
        public static ILine[] Resize(params ILine[] inputs) {
            var copy = new ILine[inputs.Length];
            var maxSize = 0;

            for (var i = 0; i < inputs.Length; i++) {
                var size = inputs[i].Segments;
                if (size > maxSize) maxSize = size;
            }

            for (var i = 0; i < inputs.Length; i++) {
                copy[i] = inputs[i].Segments == maxSize
                    ? inputs[i]
                    : inputs[i].Resize(maxSize);
            }

            return copy;
        }
    }
}
