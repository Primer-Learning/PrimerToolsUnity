using System;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public sealed record LatexChar(LatexSymbol symbol, Vector3 position, float scale = 1)
    {
        public static LatexChar Lerp(LatexChar left, LatexChar right, float t)
        {
            if (!left.IsSameSymbol(right))
                throw new ArgumentException("Can't lerp latex characters for different symbols");

            return left with {
                position = Vector3.Lerp(left.position, right.position, t),
                scale = Mathf.Lerp(left.scale, right.scale, t)
            };
        }

        public static LatexChar LerpScale(LatexChar @char, float t) =>
            @char with { scale = Mathf.Lerp(0, @char.scale, t) };


        public bool isSpriteValid => symbol is not null && symbol.isSpriteValid;


        public bool IsSameSymbol(LatexChar other) =>
            other is not null && symbol.Equals(other.symbol);

        public void Draw(Transform parent, Material material) =>
            symbol.Draw(parent, material, position, scale);

#if UNITY_EDITOR
        // for debug proposes
        public (string code, int i) source;

        public void DrawWireGizmos(Transform parent, LatexGizmoMode features = LatexGizmoMode.Nothing) =>
            symbol.DrawWireGizmos(parent, position,features);
#endif
    }
}
