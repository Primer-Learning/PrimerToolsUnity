#if UNITY_EDITOR
using System;

namespace Primer.Latex
{
    [Flags]
    public enum LatexGizmoMode
    {
        Nothing = 0b0,
        WireFrame = 0b1,
        Normals = 0b10,
        Tangents = 0b100,
        Everything = 0b111,
    }
}
#endif
