using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    [DisplayName("Primer/Generic Track")]
    [TrackClipType(typeof(GenericClip))]
    [TrackBindingType(typeof(Transform))]
    internal class GenericTrack : PrimerTrack
    {
        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip) {
            var playable = (ScriptPlayable<GenericBehaviour>)base.CreatePlayable(graph, gameObject, clip);

            // Only the TimelineClip has the actual duration (not including extrapolation) so we must grab it here.
            var behaviour = playable.GetBehaviour();

            if (behaviour is not null)
                behaviour.duration = clip.duration;

            return playable;
        }
    }
}
