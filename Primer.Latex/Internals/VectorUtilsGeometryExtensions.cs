using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    internal static class VectorUtilsGeometryExtensions
    {
        public const float SVG_PIXELS_PER_UNIT = 10f;

        // Magic number, we fiddled with it until we got the expected result
        // If a character scales up and down in the same animation increase
        // If a character transforms in a different one decrease
        // - 125 tweens opposite parenthesis
        // - 100 removes and re-appears all parenthesis
        private const float TOLERANCE = 110;


        public static bool IsSimilarEnough(this VectorUtils.Geometry geometry, VectorUtils.Geometry other)
        {
            var vert = geometry.Vertices;
            var otherVert = other.Vertices;

            if (vert.Length != otherVert.Length)
                return false;

            var len = vert.Length;

            for (var i = 0; i < len; i++) {
                var diff = vert[i] - otherVert[i];
                if (diff.sqrMagnitude > TOLERANCE)
                    return false;
            }

            return true;
        }

        public static int CalculateHashCode(this VectorUtils.Geometry geometry)
        {
            var hash = new HashCode();

            for (var i = 0; i < geometry.Vertices.Length; i++) {
                hash.Add(geometry.Vertices[i]);
            }

            return hash.ToHashCode();
        }

        public static Sprite ConvertToSprite(this VectorUtils.Geometry geometry)
        {
            var geometries = new List<VectorUtils.Geometry> {geometry};

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
