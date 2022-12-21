using System;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Latex
{
    [Serializable]
    public class TokenReplacementClip : PrimerClip<TokenReplacement>
    {
        public override ClipCaps clipCaps => ClipCaps.Extrapolation;
    }
}
