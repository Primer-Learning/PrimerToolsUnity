using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [TrackClipType(typeof(PlottedEquationClip))]
    [TrackClipType(typeof(PlottedDataClip))]
    [TrackBindingType(typeof(MeshFilter))]
    public class GraphSurfaceTrack : DeprecatedPrimerTrack
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) =>
            ScriptPlayable<GraphSurfaceMixerWithCollector>.Create(graph, inputCount);
    }
}
