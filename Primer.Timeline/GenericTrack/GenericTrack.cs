using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
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

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // Hack to set the display name of the clip to match the IClipNameProvider property
            foreach (var clip in GetClips()) {
                if (clip.asset is PrimerClip<GenericBehaviour> asset)
                    clip.displayName = asset.template.clipName;
            }

            return base.CreateTrackMixer(graph, go, inputCount);
        }
    }
}
