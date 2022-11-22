using System;
using UnityEngine;

namespace Primer.Graph
{
    public interface IGrid
    {
        int Size { get; }
        Vector3[] Points { get; }

        void RenderTo(Mesh mesh, bool bothSides);

        IGrid Resize(int newSize);
        IGrid SmoothCut(float newSize, bool fromOrigin);


        public static T Lerp<T>(T a, T b, float t) where T : IGrid {
            var maxSize = a.Size > b.Size ? a.Size : b.Size;
            var pointsPerSide = maxSize + 1;
            var length = pointsPerSide * pointsPerSide;
            var finalPoints = new Vector3[length];

            if (a.Size != maxSize) a = (T)a.Resize(maxSize);
            if (b.Size != maxSize) b = (T)b.Resize(maxSize);

            for (var i = 0; i < length; i++) {
                finalPoints[i] = Vector3.Lerp(a.Points[i], b.Points[i], t);
            }

            return (T)Activator.CreateInstance(a.GetType(), finalPoints);
        }

        /// <summary>
        ///     Use this when resizing several grids at the same time
        ///     this ensures grids don't suffer more than one transformation
        /// </summary>
        public static IGrid[] Resize(params IGrid[] inputs) {
            var copy = new IGrid[inputs.Length];
            var maxSize = 0;

            for (var i = 0; i < inputs.Length; i++) {
                var size = inputs[i].Size;
                if (size > maxSize) maxSize = size;
            }

            for (var i = 0; i < inputs.Length; i++) {
                copy[i] = inputs[i].Size == maxSize
                    ? inputs[i]
                    : inputs[i].Resize(maxSize);
            }

            return copy;
        }
    }
}
