using System;
using UnityEngine;

namespace Primer.Graph
{
    public readonly struct DiscreteGrid : IGrid
    {
        const int minSize = 1;
        public static DiscreteGrid zero = new(0);

        public Vector3[] points { get; }
        public int size { get; }

        int SideLength => size + 1;


        DiscreteGrid(Vector3[] points, int size) {
            this.points = points;
            this.size = size;
        }


        #region Public constructors
        public DiscreteGrid(int size) {
            this.size = size;
            points = new Vector3[(size + 1) * (size + 1)];
        }

        public DiscreteGrid(Vector3[] vertices) {
            var originalSize = GetSizeFromArray(vertices) - 1;

            if (originalSize.IsInteger() && originalSize >= minSize) {
                size = (int)originalSize;
                points = vertices;
                return;
            }

            var newSize = Mathf.Max(Mathf.CeilToInt(originalSize), minSize);
            var newLength = (newSize + 1) * (newSize + 1);
            var newVertices = new Vector3[newLength];

            vertices.CopyTo(newVertices, 0);

            size = newSize;
            points = newVertices;
        }
        #endregion


        #region Render to mesh
        public void RenderTo(Mesh mesh, bool bothSides = false) {
            var triangles = DefineTriangles(bothSides);
            var points = bothSides ? DuplicatedList(this.points) : this.points;

            mesh.Clear();
            mesh.vertices = points;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }
        #endregion


        #region Manipulation
        public IGrid Resize(int newSize) {
            var size = this.size;
            if (newSize == size) return this;

            var sideLength = SideLength;
            var newSideLength = newSize + 1;

            var points = this.points;
            var result = new Vector3[newSideLength * newSideLength];

            for (var y = 0; y < newSideLength; y++) {
                for (var x = 0; x < newSideLength; x++) {
                    var i = y * newSideLength + x;
                    var col = (float)x / newSize * size;
                    var row = (float)y / newSize * size;

                    result[i] = col.IsInteger() && row.IsInteger()
                        ? points[(int)row * sideLength + (int)col]
                        : QuadLerp(points, sideLength, row, col);
                }
            }

            return new DiscreteGrid(result, newSize);
        }

        public IGrid Crop(float newSize) => SmoothCut(newSize, false);
        public IGrid SmoothCut(float croppedSize, bool fromOrigin) {
            if (croppedSize == 0) return zero;

            if (croppedSize.IsInteger() && size == (int)croppedSize) {
                return this;
            }

            if (size < croppedSize) {
                throw new Exception("Crop size is bigger than grid area. Do you want IGrid.Resize()?");
            }

            var finalSize = Mathf.CeilToInt(croppedSize);
            var sideLength = SideLength;
            var length = finalSize + 1;
            var offset = fromOrigin ? sideLength - length : 0;
            var t = croppedSize.GetDecimals();

            var points = this.points;
            var copy = new Vector3[length * length];

            // Copy unchanged points, including points to lerp
            for (var x = 0; x < length; x++) {
                for (var y = 0; y < length; y++) {
                    copy[y * length + x] = points[(y + offset) * sideLength + x + offset];
                }
            }

            if (fromOrigin) {
                // Lerp corner
                copy[0] = Vector3.Lerp(copy[length + 1], copy[0], t);

                // Lerp top
                for (var x = 1; x < length; x++) {
                    copy[x] = Vector3.Lerp(copy[length + x], copy[x], t);
                }

                // Lerp left side
                for (var y = 1; y < length; y++) {
                    copy[y * length] = Vector3.Lerp(copy[y * length + 1], copy[y * length], t);
                }
            }
            else {
                var lastIndex = finalSize;

                // Lerp bottom
                for (var x = 0; x < length; x++) {
                    var y = lastIndex;
                    var a = copy[(y - 1) * length + x];
                    var b = copy[y * length + x];
                    copy[y * length + x] = Vector3.Lerp(a, b, t);
                }

                // Lerp right side
                for (var y = 0; y < length; y++) {
                    var x = lastIndex;
                    var a = copy[y * length + x - 1];
                    var b = copy[y * length + x];
                    copy[y * length + x] = Vector3.Lerp(a, b, t);
                }

                // Lerp corner
                copy[lastIndex * length + lastIndex] = QuadLerp(points, sideLength, croppedSize, croppedSize);
            }

            return new DiscreteGrid(copy, finalSize);
        }
        #endregion


        #region Triangles
        int[] DefineTriangles(bool bothSides) {
            var size = this.size;
            var sideLength = SideLength;
            var pointsCount = sideLength * sideLength;
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
                    triangles[t + 1] = v + sideLength;
                    triangles[t + 2] = v + sideLength + 1;

                    // second triangle
                    triangles[t + 3] = v;
                    triangles[t + 4] = v + sideLength + 1;
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

        static Vector3[] DuplicatedList(Vector3[] list) {
            var copy = new Vector3[list.Length * 2];
            list.CopyTo(copy, 0);
            list.CopyTo(copy, list.Length);
            return copy;
        }

        static Vector3 QuadLerp(Vector3[] points, int length, float row, float col, bool reverse = false) {
            var top = Mathf.FloorToInt(row);
            var bottom = Mathf.CeilToInt(row);
            var left = Mathf.FloorToInt(col);
            var right = Mathf.CeilToInt(col);

            var topLeft = points[top * length + left];
            var topRight = points[top * length + right];
            var bottomLeft = points[bottom * length + left];
            var bottomRight = points[bottom * length + right];

            var tx = col.GetDecimals();
            var ty = row.GetDecimals();

            var a = Vector3.Lerp(topLeft, topRight, reverse ? 1 - tx : tx);
            var b = Vector3.Lerp(bottomLeft, bottomRight, reverse ? 1 - tx : tx);

            return Vector3.Lerp(a, b, reverse ? 1 - ty : ty);
        }
        #endregion
    }
}
