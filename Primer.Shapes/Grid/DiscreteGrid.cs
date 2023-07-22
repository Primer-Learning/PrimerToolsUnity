using System;
using Primer.Graph;
using UnityEngine;

namespace Primer.Shapes
{
    public readonly struct DiscreteGrid : IGrid
    {
        public static DiscreteGrid zero = new(0);

        public Vector3[,] points { get; }
        public Vector2Int resolution => new(points.GetLength(0), points.GetLength(1));


        public DiscreteGrid(int cellsPerSide) {
            points = new Vector3[cellsPerSide + 1, cellsPerSide + 1];
        }

        public DiscreteGrid(int columns, int rows) {
            points = new Vector3[columns + 1, rows + 1];
        }

        public DiscreteGrid(Vector2Int resolution) {
            points = new Vector3[resolution.x, resolution.y];
        }

        public DiscreteGrid(Vector3[,] points) {
            this.points = points;
        }


        public IGrid ChangeResolution(Vector2Int newResolution) {
            var currentResolution = resolution;

            if (newResolution == currentResolution)
                return this;

            var result = new Vector3[newResolution.x, newResolution.y];

            for (var y = 0; y < newResolution.y; y++) {
                for (var x = 0; x < newResolution.x; x++) {
                    var coords = new Vector2(x, y) / newResolution * currentResolution;

                    result[x, y] = coords.IsInteger()
                        ? points[(int)coords.x, (int)coords.y]
                        : QuadLerp(GetQuad(points, coords), coords.GetDecimals());
                }
            }

            return new DiscreteGrid(result);
        }

        // public IGrid Crop(float newSize) => SmoothCut(newSize, false);
        public IGrid SmoothCut(Vector2 croppedSize, bool fromOrigin) {
            if (croppedSize == Vector2.zero)
                return zero;

            if (resolution == croppedSize)
                return this;

            if (croppedSize.x > resolution.x || croppedSize.y > resolution.y)
                throw new Exception("Crop size is bigger than grid area. Do you want IGrid.ChangeResolution()?");

            var finalSize = Vector2Int.CeilToInt(croppedSize);
            // var sideLength = this.sideLength;
            // var length = finalSize + 1;
            // var offset = fromOrigin ? sideLength - length : 0;
            var offset = fromOrigin ? resolution - finalSize : Vector2Int.zero;
            var t = new Vector2(croppedSize.x.GetDecimals(), croppedSize.y.GetDecimals());
            var copy = new Vector3[finalSize.x, finalSize.y];

            // Copy unchanged points, including points to lerp
            for (var x = 0; x < finalSize.x; x++) {
                for (var y = 0; y < finalSize.y; y++) {
                    copy[x, y] = points[x + offset.x, y + offset.y];
                }
            }

            if (fromOrigin) {
                // Lerp corner
                copy[0, 0] = QuadLerp(GetQuad(points, resolution - croppedSize), Vector2.one - t);

                // Lerp left side
                for (var x = 1; x < finalSize.x; x++)
                    copy[x, 0] = Vector3.Lerp(copy[x, 1], copy[x, 0], t.x);

                // Lerp top
                for (var y = 1; y < finalSize.y; y++)
                    copy[0, y] = Vector3.Lerp(copy[1, y], copy[0, y], t.y);
            }
            else {
                var lastIndex = finalSize - Vector2Int.one;

                // Lerp right side
                for (var x = 0; x < lastIndex.x; x++)
                    copy[x, lastIndex.y] = Vector3.Lerp(copy[x, lastIndex.y - 1], copy[x, lastIndex.y], t.y);

                // Lerp bottom
                for (var y = 0; y < lastIndex.y; y++)
                    copy[lastIndex.x, y] = Vector3.Lerp(copy[lastIndex.x - 1, y], copy[lastIndex.x, y], t.x);

                // Lerp corner
                copy[lastIndex.x, lastIndex.y] = QuadLerp(GetQuad(points, croppedSize), t);
            }

            return new DiscreteGrid(copy);
        }

        private static Vector3 QuadLerp((Vector3 x0y0, Vector3 x0y1, Vector3 x1y0, Vector3 x1y1) quad, Vector2 t)
        {
            return Vector3.Lerp(
                Vector3.Lerp(quad.x0y0, quad.x1y0, t.x),
                Vector3.Lerp(quad.x0y1, quad.x1y1, t.x),
                t.y
            );
        }

        private static (Vector3, Vector3, Vector3, Vector3) GetQuad(Vector3[,] points, Vector2 location)
        {
            var floorX = Mathf.FloorToInt(location.x);
            var floorY = Mathf.FloorToInt(location.y);
            var ceilX = Mathf.CeilToInt(location.x);
            var ceilY = Mathf.CeilToInt(location.y);

            if (ceilX >= points.GetLength(0) || ceilY >= points.GetLength(1))
                throw new Exception("WAT");

            return (
                x0y0: points[floorX, floorY],
                x0y1: points[floorX, ceilY],
                x1y0: points[ceilX, floorY],
                x1y1: points[ceilX, ceilY]
            );
        }
    }
}
