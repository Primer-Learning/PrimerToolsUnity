using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public abstract class PrimerTrack : TrackAsset
    {
        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip) {
            var playable = base.CreatePlayable(graph, gameObject, clip);

            var behaviour = ((ScriptPlayable<PrimerPlayable>)playable).GetBehaviour();

            if (behaviour is not null) {
                behaviour.duration = clip.duration;
            }

            return playable;
        }
    }
}
