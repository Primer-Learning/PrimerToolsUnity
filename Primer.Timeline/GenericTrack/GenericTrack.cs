using System.Reflection;
using Primer.Timeline.GenericTrack;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// ReSharper disable once CheckNamespace
// Namespace does not correspond to file location, should be: 'Primer.Timeline'
// We use FakeUnityEngine namespace because if "UnityEngine" is part of the namespace Unity allow us
//  to show this track without submenu
namespace Primer.Timeline.FakeUnityEngine
{
    [TrackClipType(typeof(GenericClip))]
    [TrackBindingType(typeof(Transform))]
    internal class GenericTrack : PrimerTrack
    {
        // protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip) {
        //     var playable = (ScriptPlayable<GenericBehaviour>)base.CreatePlayable(graph, gameObject, clip);
        //
        //     // Only the TimelineClip has the actual duration (not including extrapolation) so we must grab it here.
        //     var behaviour = playable.GetBehaviour();
        //
        //     if (behaviour is not null)
        //         behaviour.duration = clip.duration;
        //
        //     return playable;
        // }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var clip in GetClips()) {
                if (clip.asset is not PrimerClip<GenericBehaviour> asset)
                    continue;

                // HACK: to set the display name of the clip to match the clipName property
                clip.displayName = asset.template.clipName;

                // HACK: pass the clip values to the GenericBehaviour
                asset.template.start = (float)clip.start;
                asset.template.duration = (float)clip.duration;
            }

            return ScriptPlayable<GenericMixer>.Create(graph, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);

            // clip.postExtrapolationMode = TimelineClip.ClipExtrapolation.Hold;
            //
            // HACK: Property's setter above is internal (why? ask Unity), so we use reflection to set it.
            const BindingFlags FLAGS = BindingFlags.NonPublic | BindingFlags.Instance;
            clip.GetType()
                .GetMethod("set_postExtrapolationMode", FLAGS)
                ?.Invoke(clip, new object[] { TimelineClip.ClipExtrapolation.Hold });
        }
    }
}
