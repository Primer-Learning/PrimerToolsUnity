using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    static internal class VectorUtilsGeometryExtensions
    {
        public const float SVG_PIXELS_PER_UNIT = 10f;

        public static int CalculateHashCode(this VectorUtils.Geometry geometry)
        {
            var hash = new HashCode();

            for (var i = 0; i < geometry.Vertices.Length; i++) {
                hash.Add(geometry.Vertices[i]);
            }

            return hash.ToHashCode();
        }

        public static Vector2 CalculatePosition(this VectorUtils.Geometry geometry, Vector2 offset)
        {
            var position = VectorUtils.Bounds(geometry.TransformVertices()).center - offset;

            // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
            // rather than just one at a time.
            position.y = -position.y;

            return position;
        }

        public static Sprite ConvertToSprite(this VectorUtils.Geometry geometry)
        {
            var geometries = new List<VectorUtils.Geometry> { geometry };

            return VectorUtils.BuildSprite(
                geometries,
                SVG_PIXELS_PER_UNIT,
                VectorUtils.Alignment.Center,
                Vector2.zero,
                128,
                true
            );
        }

        public static IEnumerable<Vector2> TransformVertices(this VectorUtils.Geometry geometry)
        {
            return geometry.Vertices.Select(vertex => geometry.WorldTransform * vertex / SVG_PIXELS_PER_UNIT);
        }

        #region Geometry comparison
        // Magic number, we fiddled with it until we got the expected result
        private const float TOLERANCE = 0.5f;

        private static bool AreCloseEnough(Vector2 left, Vector2 right) => (left - right).sqrMagnitude < TOLERANCE;

        public static bool IsSimilarEnough(this VectorUtils.Geometry leftGeometry, VectorUtils.Geometry rightGeometry)
        {
            if (Math.Abs(leftGeometry.Vertices.Length - rightGeometry.Vertices.Length) > 1) {
                Debug.Log(
                    $"Vertex count doesn't match {leftGeometry.Vertices.Length} != {rightGeometry.Vertices.Length}"
                );

                return false;
            }

            var left = leftGeometry.NormalizeVectors();
            var right = rightGeometry.NormalizeVectors();
            var leftCursor = 0;
            var rightCursor = 0;

            for (; (leftCursor < left.Count) && (rightCursor < right.Count); leftCursor++, rightCursor++) {
                if (AreCloseEnough(left[leftCursor], right[rightCursor]))
                    continue;

                if ((right.Count > left.Count) && AreCloseEnough(left[leftCursor], right[rightCursor + 1])) {
                    rightCursor++;
                    continue;
                }

                if ((left.Count > right.Count) && AreCloseEnough(left[leftCursor + 1], right[rightCursor])) {
                    leftCursor++;
                    continue;
                }

                return false;
            }

            return (leftCursor == left.Count) && (rightCursor == right.Count);
        }

        public static List<Vector2> NormalizeVectors(this VectorUtils.Geometry geometry)
        {
            var verts = geometry.Vertices;
            var center = VectorUtils.Bounds(verts).center;

            return geometry.Vertices
                           .Select(vec => vec - center)
                           .OrderBy(vec => vec, new VectorComparer())
                           .ToList();
        }

        internal class VectorComparer : IComparer<Vector2>
        {
            // Max tolerated difference
            private const float THRESHOLD = 0.00001f;

            public int Compare(Vector2 left, Vector2 right)
            {
                var xDiff = left.x - right.x;

                if (Mathf.Abs(xDiff) > THRESHOLD)
                    return xDiff < 0 ? -1 : 1;

                var yDiff = left.y - right.y;

                if (Mathf.Abs(yDiff) > THRESHOLD)
                    return yDiff < 0 ? -1 : 1;

                if ((yDiff == 0) && (xDiff == 0))
                    return 0;

                if (Mathf.Abs(xDiff) >= Mathf.Abs(yDiff))
                    return xDiff < 0 ? -1 : 1;

                return yDiff < 0 ? -1 : 1;
            }
        }
        #endregion
    }
}
