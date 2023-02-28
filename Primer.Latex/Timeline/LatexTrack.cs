using Primer.Animation;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// HACK: We use FakeUnityEngine namespace because if "UnityEngine" is part of the namespace Unity allow us
//  to show this track without submenu
// ReSharper disable once CheckNamespace
namespace Primer.Latex.FakeUnityEngine
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
