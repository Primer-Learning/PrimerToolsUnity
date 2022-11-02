using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    internal class AdapterClip : PlayableAsset, ITimelineClipAsset
    {
        [SerializeReference] public ScrubbableAdapter adapter;

        public ClipCaps clipCaps => ClipCaps.Extrapolation;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            if (adapter is null || adapter.errors.Count > 0) return Playable.Null;

            var playable = ScriptPlayable<AdapterBehaviour>.Create(graph);

            adapter.resolver = graph.GetResolver();

            var behaviour = playable.GetBehaviour();
            behaviour.adapter = adapter;

            return playable;
        }
    }
}
