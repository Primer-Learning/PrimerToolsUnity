using System;
using System.Collections.Generic;
using Primer.Timeline;
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

        public override UnityEngine.Playables.Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<Playable>.Create(graph);
            var behaviour = playable.GetBehaviour();
            var renderer = transformTo.Resolve(graph.GetResolver());

            behaviour.transition = new LatexTransitionState(renderer.transform, transitions);

            return playable;
        }

        [Serializable]
        public class Playable : PrimerPlayable
        {
            internal LatexTransitionState transition;
        }
    }
}
