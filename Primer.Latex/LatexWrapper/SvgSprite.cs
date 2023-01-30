using System;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    public record SvgSprite(Rect bounds, Vector2 position, Sprite sprite)
    {
        public LatexChar ToLatexChar() => new(ConvertToMesh(sprite), bounds, position);

        private static Mesh ConvertToMesh(Sprite sprite)
        {
            if (sprite == null) return null;

            var normals = new Vector3(0f, 0f, -1f);
            var tangents = new Vector4(1f, 0f, 0f, -1f);

            return new Mesh {
                vertices = Array.ConvertAll(sprite.vertices, i => (Vector3)i),
                triangles = Array.ConvertAll(sprite.triangles, i => (int)i),
                normals = Enumerable.Repeat(normals, sprite.vertices.Length).ToArray(),
                tangents = Enumerable.Repeat(tangents, sprite.vertices.Length).ToArray(),
                uv = sprite.vertices,
            };
        }
    }
}
