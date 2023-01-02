using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Latex
{
    public class LatexTransformerClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<LatexRenderer> transformTo;
        public List<TransitionType> transitions = new();

        public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<LatexTransformer>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.transformTo = transformTo.Resolve(graph.GetResolver());
            behaviour.transitions = transitions;

            return playable;
        }
    }
}
