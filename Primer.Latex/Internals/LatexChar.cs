using System;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public sealed record LatexChar(LatexSymbol symbol, Vector3 position, float scale = 1)
    {
        public static LatexChar Lerp(LatexChar a, LatexChar b, float t)
        {
            if (!a.IsSameSymbol(b))
                throw new ArgumentException("Can't lerp latex characters for different symbols");

            return a with {
                position = Vector3.Lerp(a.position, b.position, t),
                scale = Mathf.Lerp(a.scale, b.scale, t)
            };
        }

        public static LatexChar LerpScale(LatexChar a, float t)
        {
            return a with { scale = Mathf.Lerp(0, a.scale, t) };
        }


        public bool isSpriteValid => symbol is not null && symbol.isSpriteValid;


        public bool IsSameSymbol(LatexChar other) =>
            other is not null && symbol.Equals(other.symbol);

        public void Draw(Transform parent, Material material) =>
            symbol.Draw(parent, material, position, scale);

#if UNITY_EDITOR
        public void DrawWireGizmos(Transform parent, LatexGizmoMode features = LatexGizmoMode.Nothing) =>
            symbol.DrawWireGizmos(parent, position,features);
#endif
    }
}
