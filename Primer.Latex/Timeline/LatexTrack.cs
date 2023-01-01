using System.ComponentModel;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Latex
{
    [DisplayName("Primer / Latex track")]
    [TrackClipType(typeof(GroupTransformerClip))]
    [TrackBindingType(typeof(LatexRenderer))]
    public class GraphLineTrack : PrimerTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<LatexMixer>.Create(graph, inputCount);
        }
    }
}
