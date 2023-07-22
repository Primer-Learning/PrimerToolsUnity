using Primer.Shapes;
using UnityEngine;

namespace Primer.Graph
{
    public interface IGrid
    {
        Vector2Int resolution { get; }
        Vector3[,] points { get; }

        IGrid ChangeResolution(Vector2Int newResolution);
        IGrid SmoothCut(Vector2 newSize, bool fromOrigin);


        public static IGrid Lerp(IGrid a, IGrid b, float t)
        {
            var maxResolution = Vector2Int.Max(a.resolution, b.resolution);
            var finalPoints = new Vector3[maxResolution.x, maxResolution.y];

            if (a.resolution != maxResolution) a = a.ChangeResolution(maxResolution);
            if (b.resolution != maxResolution) b = b.ChangeResolution(maxResolution);

            for (var x = 0; x < maxResolution.x; x++) {
                for (var y = 0; y < maxResolution.y; y++) {
                    finalPoints[x, y] = Vector3.Lerp(a.points[x, y], b.points[x, y], t);
                }
            }

            return new DiscreteGrid(finalPoints);
        }

        /// <summary>
        ///     Use this when resizing several grids at the same time
        ///     this ensures grids don't suffer more than one transformation
        /// </summary>
        public static IGrid[] SameResolution(params IGrid[] inputs) {
            var sameResolution = new IGrid[inputs.Length];
            var maxResolution = Vector2Int.zero;

            for (var i = 0; i < inputs.Length; i++) {
                maxResolution = Vector2Int.Max(maxResolution, inputs[i].resolution);
            }

            for (var i = 0; i < inputs.Length; i++) {
                sameResolution[i] = inputs[i].ChangeResolution(maxResolution);
            }

            return sameResolution;
        }

        // For convenience
        public static (IGrid, IGrid) SameResolution(IGrid a, IGrid b) {
            var maxResolution = Vector2Int.Max(a.resolution, b.resolution);
            return (a.ChangeResolution(maxResolution), b.ChangeResolution(maxResolution));
        }
    }
}
