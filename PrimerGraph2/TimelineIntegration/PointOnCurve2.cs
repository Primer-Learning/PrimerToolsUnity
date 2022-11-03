using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    public class PointOnCurve2 : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Extrapolation;

        public ExposedReference<Transform> prefab;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            var prefab = this.prefab.Resolve(graph.GetResolver());

            var playable = ScriptPlayable<GraphBehaviour>.Create(graph);

            // var behaviour = playable.GetBehaviour();
            // adapter.resolver = graph.GetResolver();
            // behaviour.adapter = adapter;

            return playable;
        }
    }
}
