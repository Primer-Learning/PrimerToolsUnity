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

        [HideInInspector]
        public List<TransitionType> transitions = new();


        public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;


        public override UnityEngine.Playables.Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<Playable>.Create(graph);
            var behaviour = playable.GetBehaviour();
            var renderer = transformTo.Resolve(graph.GetResolver());

            if (renderer is null) {
                throw new NullReferenceException(
                    $"Clip {nameof(LatexTransformerClip)}'s {nameof(transformTo)} property is null"
                );
            }

            behaviour.state = new LatexTransitionState(renderer, transitions);

            return playable;
        }


        [Serializable]
        public class Playable : PrimerPlayable
        {
            internal LatexTransitionState state;
        }
    }
}
