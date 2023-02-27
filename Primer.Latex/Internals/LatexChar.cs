using System;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public class LatexChar
    {
        public static LatexChar Lerp(LatexChar a, LatexChar b, float t) => new(
            mesh: a.mesh,
            bounds: a.bounds,
            position: Vector3.Lerp(a.position, b.position, t),
            scale: Mathf.Lerp(a.scale, b.scale, t)
        );


        public Mesh mesh { get; init; }
        public Rect bounds { get; init; }
        public Vector3 position { get; init; }
        public float scale { get; init; }


        internal LatexChar(VectorUtils.Geometry geometry, Vector2 offset)
        {
            var sprite = geometry.ConvertToSprite();
            mesh = ConvertToMesh(sprite);

            bounds = VectorUtils.Bounds(geometry.TransformVertices());
            var center = bounds.center - offset;

            // This matches the way flipYAxis would work in BuildSprite if we gave it all of the geometry in the SVG
            // rather than just one at a time.
            position = new Vector2(center.x, -center.y);
        }

        internal LatexChar(Mesh mesh,
            Rect bounds,
            Vector3 position, float scale = 1f)
        {
            this.mesh = mesh;
            this.bounds = bounds;
            this.position = position;
            this.scale = scale;
        }


        public void Draw(Transform parent, Material material)
        {
            var positionMod = Matrix4x4.Translate(position);
            var scaleMod = Matrix4x4.Scale(Vector3.one * scale);
            var matrix = parent.localToWorldMatrix * positionMod * scaleMod;
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }

        private static Mesh ConvertToMesh(Sprite sprite)
        {
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
