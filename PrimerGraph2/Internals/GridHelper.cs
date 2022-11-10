using System;
using UnityEngine;
using UnityEngine.Internal;

namespace Primer.Graph
{
    public static class GridHelper
    {
        #region Grid creation
        public static Vector3[] CreateContinuousGridVectors(int gridSize, float cellSize = 1) {
            var pointsPerSide = gridSize + 1;
            var vertices = new Vector3[pointsPerSide * pointsPerSide];
            var v = 0;

            // create vertex grid
            for (var x = 0; x < pointsPerSide; x++) {
                for (var y = 0; y < pointsPerSide; y++) {
                    vertices[v] = new Vector3(x * cellSize, 0, y * cellSize);
                    v++;
                }
            }

            return vertices;
        }

        public static int[] CreateContinuousGridTriangles(int gridSize, bool bothSides = false) {
            var pointsPerSide = gridSize + 1;
            var trianglesPerSquare = bothSides ? 4 : 2;
            var edgesPerSquare = trianglesPerSquare * 3;
            var triangles = new int[gridSize * gridSize * edgesPerSquare];
            var v = 0;
            var t = 0;

            // setting each square's triangles
            for (var x = 0; x < gridSize; x++) {
                for (var y = 0; y < gridSize; y++) {
                    // first triangle
                    triangles[t] = v;
                    triangles[t + 1] = v + pointsPerSide + 1;
                    triangles[t + 2] = v + 1;

                    // second triangle
                    triangles[t + 3] = v;
                    triangles[t + 4] = v + pointsPerSide;
                    triangles[t + 5] = v + pointsPerSide + 1;

                    if (bothSides) {
                        // first triangle back
                        triangles[t + 6] = triangles[t];
                        triangles[t + 7] = triangles[t + 2];
                        triangles[t + 8] = triangles[t + 1];

                        // second triangle back
                        triangles[t + 9] = triangles[t + 3];
                        triangles[t + 10] = triangles[t + 5];
                        triangles[t + 11] = triangles[t + 4];
                    }

                    v++;
                    t += edgesPerSquare;
                }
            }

            return triangles;
        }
        #endregion


        #region Grid transformation
        public static Vector3[] EnsureIsGrid(
            Vector3[] vertices,
            out int size,
            [DefaultValue("Vector3.zero")]
            Vector3? fillValue = null,
            int minSize = 1
        ) {
            var originalSize = GetGridSize(vertices);

            if (IsInteger(originalSize) && originalSize >= minSize) {
                size = (int)originalSize - 1;
                return vertices;
            }

            var fill = fillValue ?? Vector3.zero;
            var newSize = Mathf.Max(Mathf.CeilToInt(originalSize), minSize);
            var newLength = newSize * newSize;
            var newVertices = new Vector3[newLength];

            vertices.CopyTo(newVertices, 0);

            // Fill places required to make a grid out of it
            for (var i = vertices.Length; i < newLength; i++) {
                newVertices[i] = fill;
            }

            size = newSize;
            return newVertices;
        }

        public static Vector3[] ResizeGrid(Vector3[] vertices, int newSize) {
            var grid = EnsureIsGrid(vertices, out var size);
            if (size == newSize) return grid;

            var result = new Vector3[newSize * newSize];

            for (var x = 0; x < newSize; x++) {
                for (var y = 0; y < newSize; y++) {
                    var i = y * newSize + x;
                    var col = (float)x / newSize * size;
                    var row = (float)y / newSize * size;

                    result[i] = IsInteger(col) && IsInteger(row)
                        ? grid[(int)row + (int)col * size]
                        : QuadLerp(grid, size, row, col);
                }
            }

            return result;
        }

        public static Vector3[] CropGrid(Vector3[] grid, int size, float croppedSize) {
            if (size == croppedSize) return grid;
            if (size < croppedSize) throw new Exception("bruh");

            var lastIndex = Mathf.FloorToInt(croppedSize);
            var finalSize = Mathf.CeilToInt(croppedSize);
            var t = croppedSize % 1;

            var copy = new Vector3[finalSize * finalSize];

            // Copy unchanged points
            for (var x = 0; x < finalSize; x++) {
                for (var y = 0; y < finalSize; y++) {
                    copy[y * finalSize + x] = grid[y * size + x];
                }
            }

            // Lerp right side
            for (var x = 0; x < finalSize; x++) {
                var y = lastIndex;
                var a = grid[y * size + x];
                var b = grid[(y + 1) * size + x];
                copy[y * finalSize + x] = Vector3.Lerp(a, b, t);
            }

            // Lerp bottom
            for (var y = 0; y < finalSize; y++) {
                var x = lastIndex;
                var a = grid[y * size + x];
                var b = grid[y * size + x + 1];
                copy[y * finalSize + x] = Vector3.Lerp(a, b, t);
            }

            // Lerp corner
            copy[lastIndex * finalSize + lastIndex] = QuadLerp(grid, size, croppedSize, croppedSize);

            return copy;
        }
        #endregion


        #region Helpers
        public static float GetGridSize(Vector3[] vertices) => Mathf.Sqrt(vertices.Length);
        static float GetDecimals(float value) => value % 1;
        static bool IsInteger(float value) => value % 1 == 0;

        static Vector3 QuadLerp(Vector3[] grid, int size, float row, float col) {
            var top = Mathf.FloorToInt(row);
            var bottom = Mathf.CeilToInt(row);
            var left = Mathf.FloorToInt(col);
            var right = Mathf.CeilToInt(col);

            var topLeft = grid[top * size + left];
            var topRight = grid[top * size + right];
            var bottomLeft = grid[bottom * size + left];
            var bottomRight = grid[bottom * size + right];

            var tx = GetDecimals(col);
            var ty = GetDecimals(row);

            var a = Vector3.Lerp(topLeft, topRight, tx);
            var b = Vector3.Lerp(bottomLeft, bottomRight, tx);

            return Vector3.Lerp(a, b, ty);
        }
        #endregion
    }
}
