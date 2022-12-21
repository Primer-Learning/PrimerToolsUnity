
using System;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Latex
{
    [Serializable]
    public class TokenReplacementClip : PrimerClip<TokenReplacement>
    {
        private readonly LatexProcessor processor = LatexProcessor.GetInstance();

        public override ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            template.processor = processor;
            return base.CreatePlayable(graph, owner);
        }
    }
}
