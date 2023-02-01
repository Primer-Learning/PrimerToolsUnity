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
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var clip in GetClips()) {
                if (clip.asset is not GenericClip asset)
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

            clip.duration = 1;

            // clip.postExtrapolationMode = TimelineClip.ClipExtrapolation.Hold;
            //
            // HACK: Property's setter above is internal (why? ask Unity), so we use reflection to set it.
            const BindingFlags PRIVATE_INSTANCE = BindingFlags.NonPublic | BindingFlags.Instance;
            clip.GetType()
                .GetMethod("set_postExtrapolationMode", PRIVATE_INSTANCE)
                ?.Invoke(clip, new object[] { TimelineClip.ClipExtrapolation.Hold });
        }
    }
}
