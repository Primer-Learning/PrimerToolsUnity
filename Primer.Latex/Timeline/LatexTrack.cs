using System.ComponentModel;
using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Latex
{
    [TrackClipType(typeof(LatexTransformerClip))]
    [TrackBindingType(typeof(LatexRenderer))]
    public class LatexTrack : PrimerTrack
    {
        public AnimationCurve curve = IPrimerAnimation.cubic;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<LatexMixer>.Create(graph, inputCount);
            playable.GetBehaviour().curve = curve;
            return playable;
        }
    }
}
