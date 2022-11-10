using System;
using System.ComponentModel;
using UnityEngine;

namespace Primer.Graph
{
    public readonly struct ContinuousGrid : IGrid
    {
        #region Static

        public static ContinuousGrid zero = new(1);

        public static IGrid Lerp(IGrid a, IGrid b, float weight) {
            var maxSize = Math.Max(a.Size, b.Size);
            var pointsPerSide = maxSize + 1;
            var finalPoints = new Vector3[pointsPerSide * pointsPerSide];

            if (a.Size != maxSize) a = a.Resize(maxSize);
            if (b.Size != maxSize) b = b.Resize(maxSize);

            for (var i = 0; i < pointsPerSide; i++) {
                finalPoints[i] = Vector3.Lerp(a.Points[i], b.Points[i], weight);
            }

            return new ContinuousGrid(finalPoints);
        }
        #endregion


        public Vector3[] Points { get; }
        public int Size { get; }

        int PointsPerSide => Size + 1;
        int PointsCount => PointsPerSide * PointsPerSide;


        ContinuousGrid(Vector3[] points, int size) {
            Points = points;
            Size = size;
        }


        #region Public constructors
        public ContinuousGrid(int size, Func<float, float, Vector3> filler = null) {
            var points = new Vector3[(size + 1) * (size + 1)];
            var v = 0;

            for (var x = 0; x < size; x++) {
                for (var y = 0; y < size; y++) {
                    points[v++] = filler == null
                        ? Vector3.zero
                        : filler((float)x / size, (float)y / size);
                }
            }

            Size = size;
            Points = points;
        }

        public ContinuousGrid(
            Vector3[] vertices,
            [DefaultValue("Vector3.zero")]
            Vector3? fillValue = null,
            int minSize = 1
        ) {
            var originalSize = GetSizeFromArray(vertices) - 1;

            if (IsInteger(originalSize) && originalSize >= minSize) {
                Size = (int)originalSize;
                Points = vertices;
                return;
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

            Size = newSize;
            Points = newVertices;
        }
        #endregion


        #region Render to mesh
        public void RenderTo(Mesh mesh, bool bothSides = false) {
            var triangles = DefineTriangles(bothSides);
            var points = bothSides ? DuplicatedList(Points) : Points;

            mesh.Clear();
            mesh.vertices = points;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
        #endregion


        #region Manipulation
        public IGrid Resize(int newSize) {
            if (newSize == Size) return this;

            var size = Size;
            var points = Points;
            var lastIndex = newSize;
            var pointsPerSide = newSize + 1;
            var result = new Vector3[pointsPerSide * pointsPerSide];

            for (var x = 0; x < pointsPerSide; x++) {
                for (var y = 0; y < pointsPerSide; y++) {
                    var i = y * lastIndex + x;
                    var col = (float)x / newSize * Size;
                    var row = (float)y / newSize * Size;

                    // BUG IN THIS FUNCTION
                    result[i] = IsInteger(col) && IsInteger(row)
                        ? points[(int)row * size + (int)col]
                        : QuadLerp(points, size, row, col);
                }
            }

            return new ContinuousGrid(result, newSize);
        }

        public IGrid Crop(float croppedSize) {
            if (IsInteger(croppedSize) && Size == (int)croppedSize) return this;
            if (Size < croppedSize) {
                throw new Exception("bruh");
            }

            var finalSize = Mathf.CeilToInt(croppedSize);
            var lastIndex = finalSize;
            var pps = PointsPerSide;
            var pointsCount = finalSize + 1;
            var t = croppedSize % 1;

            var points = Points;
            var copy = new Vector3[pointsCount * pointsCount];

            // Copy unchanged points
            for (var x = 0; x < pointsCount; x++) {
                for (var y = 0; y < pointsCount; y++) {
                    copy[y * pointsCount + x] = points[y * pps + x];
                }
            }

            // Lerp bottom
            for (var x = 0; x < pointsCount; x++) {
                var y = lastIndex;
                var a = points[(y - 1) * pps + x];
                var b = points[y * pps + x];
                copy[y * pointsCount + x] = Vector3.Lerp(a, b, t);
            }

            // Lerp right side
            for (var y = 0; y < pointsCount; y++) {
                var x = lastIndex;
                var a = points[y * pps + x - 1];
                var b = points[y * pps + x];
                copy[y * pointsCount + x] = Vector3.Lerp(a, b, t);
            }

            // Lerp corner
            copy[lastIndex * pointsCount + lastIndex] = QuadLerp(points, Size, croppedSize, croppedSize);

            return new ContinuousGrid(copy, finalSize);
        }
        #endregion


        #region Triangles
        int[] DefineTriangles(bool bothSides) {
            var size = Size;
            var pointsPerSide = PointsPerSide;
            var pointsCount = PointsCount;
            var trianglesPerSquare = bothSides ? 4 : 2;
            var edgesPerSquare = trianglesPerSquare * 3;
            var triangles = new int[size * size * edgesPerSquare];
            var v = 0;
            var t = 0;

            // setting each square's triangles
            for (var x = 0; x < size; x++) {
                for (var y = 0; y < size; y++) {
                    // first triangle
                    triangles[t] = v;
                    triangles[t + 1] = v + pointsPerSide;
                    triangles[t + 2] = v + pointsPerSide + 1;

                    // second triangle
                    triangles[t + 3] = v;
                    triangles[t + 4] = v + pointsPerSide + 1;
                    triangles[t + 5] = v + 1;

                    if (bothSides) {
                        // first triangle back
                        triangles[t + 6] = pointsCount + triangles[t];
                        triangles[t + 7] = pointsCount + triangles[t + 2];
                        triangles[t + 8] = pointsCount + triangles[t + 1];

                        // second triangle back
                        triangles[t + 9] = pointsCount + triangles[t + 3];
                        triangles[t + 10] = pointsCount + triangles[t + 5];
                        triangles[t + 11] = pointsCount + triangles[t + 4];
                    }

                    t += edgesPerSquare;
                    v++;
                }
                v++;
            }

            return triangles;
        }
        #endregion


        #region Helpers
        static float GetSizeFromArray(Vector3[] vertices) => Mathf.Sqrt(vertices.Length);
        static bool IsInteger(float value) => value % 1 == 0;
        static float GetDecimals(float value) => value % 1;

        static Vector3[] DuplicatedList(Vector3[] list) {
            var copy = new Vector3[list.Length * 2];
            list.CopyTo(copy, 0);
            list.CopyTo(copy, list.Length);
            return copy;
        }

        static Vector3 QuadLerp(Vector3[] grid, int size, float row, float col) {
            var pointsPerSize = size + 1;
            var top = Mathf.FloorToInt(row);
            var bottom = Mathf.CeilToInt(row);
            var left = Mathf.FloorToInt(col);
            var right = Mathf.CeilToInt(col);

            var topLeft = grid[top * pointsPerSize + left];
            var topRight = grid[top * pointsPerSize + right];
            var bottomLeft = grid[bottom * pointsPerSize + left];
            var bottomRight = grid[bottom * pointsPerSize + right];

            var tx = GetDecimals(col);
            var ty = GetDecimals(row);

            var a = Vector3.Lerp(topLeft, topRight, tx);
            var b = Vector3.Lerp(bottomLeft, bottomRight, tx);

            return Vector3.Lerp(a, b, ty);
        }
        #endregion
    }
}
