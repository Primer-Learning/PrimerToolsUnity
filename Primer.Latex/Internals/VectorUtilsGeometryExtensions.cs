using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    static internal class VectorUtilsGeometryExtensions
    {
        public const float SVG_PIXELS_PER_UNIT = 10f;

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
    }
}
