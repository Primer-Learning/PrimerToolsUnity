using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    static internal class VectorUtilsGeometryExtensions
    {
        public const float SVG_PIXELS_PER_UNIT = 10f;

        // Magic number, we fiddled with it until we got the expected result
        // If a character scales up and down in the same animation: increase
        // If a character transforms takes the place of a different one: decrease
        // - 110 tweens opposite parenthesis
        // - 105 removes and re-appears all parenthesis
        //
        // 109.136 is the difference between a left and a right oriented parenthesis
        private const float VECTOR_DISTANCE_TOLERANCE = 109.136f;
        private const int VERTICES_COUNT_TOLERANCE = 0;


        /// <summary>
        ///     This method defines if two geometries are "visually" the same
        ///     Fixes and improvements in LaTeX character transformations will probably be applied here
        /// </summary>
        public static bool IsSimilarEnough(this VectorUtils.Geometry geometry, VectorUtils.Geometry other)
        {
            var left = geometry.Vertices;
            var right = other.Vertices;

            var verticesCountDelta = Mathf.Abs(left.Length - right.Length);
            if (verticesCountDelta > VERTICES_COUNT_TOLERANCE) return false;

            var len = left.Length;

            for (var i = 0; i < len; i++) {
                var diff = left[i] - right[i];
                if (diff.sqrMagnitude >= VECTOR_DISTANCE_TOLERANCE) return false;
            }

            return true;
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

        public static Vector2 CalculatePosition(this VectorUtils.Geometry geometry, Vector2 offset)
        {
            var position = VectorUtils.Bounds(geometry.TransformVertices()).center - offset;

            // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
            // rather than just one at a time.
            position.y = -position.y;

            return position;
        }

        public static IEnumerable<Vector2> TransformVertices(this VectorUtils.Geometry geometry)
        {
            return geometry.Vertices.Select(vertex => geometry.WorldTransform * vertex / SVG_PIXELS_PER_UNIT);
        }
    }
}
