using System;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    internal struct SerializableLatexChar
    {
        public readonly (float x, float y) position;
        public readonly (float x, float y, float w, float h) bounds;
        public readonly SerializableMesh mesh;

        public SerializableLatexChar(LatexChar latexChar)
        {
            mesh = new SerializableMesh(latexChar.mesh);
            position = (latexChar.position.x, latexChar.position.y);

            bounds = (
                x: latexChar.bounds.xMin,
                y: latexChar.bounds.yMin,
                w: latexChar.bounds.width,
                h: latexChar.bounds.height
            );
        }

        public LatexChar ToLatexChar() => new(
            // This should never be no situation where this is null but...
            // Unity makes it possible 🤦
            mesh?.GetMesh(),
            new Rect(bounds.x, bounds.y, bounds.w, bounds.h),
            new Vector2(position.x, position.y)
        );
    }
}
