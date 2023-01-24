using System.ComponentModel;
using Primer.Timeline;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [TrackClipType(typeof(PlottedEquationClip))]
    [TrackClipType(typeof(PlottedDataClip))]
    [TrackBindingType(typeof(Polyline))]
    public class GraphLineTrack : PrimerTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) =>
            ScriptPlayable<GraphLineMixer>.Create(graph, inputCount);
    }
}
