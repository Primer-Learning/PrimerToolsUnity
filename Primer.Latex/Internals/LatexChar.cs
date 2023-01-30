using System;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public sealed record LatexChar(Mesh mesh, Rect bounds, Vector3 position, float scale = 1)
    {
        public static LatexChar Lerp(LatexChar a, LatexChar b, float t) => a with {
            position = Vector3.Lerp(a.position, b.position, t),
            scale = Mathf.Lerp(a.scale, b.scale, t),
        };

        public static LatexChar LerpScale(LatexChar a, float t) => a with {
            scale = Mathf.Lerp(0, a.scale, t),
        };

        public void Draw(Transform parent, Material material)
        {
            var positionMod = Matrix4x4.Translate(position);
            var scaleMod = Matrix4x4.Scale(Vector3.one * scale);
            var matrix = parent.localToWorldMatrix * positionMod * scaleMod;
            Graphics.DrawMesh(mesh, matrix, material, 0);
        }
    }
}
