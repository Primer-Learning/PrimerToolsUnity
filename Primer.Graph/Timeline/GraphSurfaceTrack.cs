using System.ComponentModel;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [DisplayName("Primer / Surface Track")]
    [TrackClipType(typeof(PlotEquationClip))]
    [TrackClipType(typeof(PlotDataClip))]
    [TrackBindingType(typeof(MeshFilter))]
    public class GraphSurfaceTrack : PrimerTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) =>
            ScriptPlayable<GraphSurfaceMixer>.Create(graph, inputCount);
    }
}
