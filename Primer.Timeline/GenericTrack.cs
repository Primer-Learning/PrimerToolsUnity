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
    internal class GenericTrack : TrackAsset
    {
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver) {
            base.GatherProperties(director, driver);

            foreach (var clip in GetClips()) {
                if (clip.asset is not GenericClip asset)
                    continue;

                var behaviour = asset.template;

                if (behaviour is IExposedReferenceResolver container)
                    container.resolver = director;

                if (behaviour is not IPropertyModifier modifier)
                    continue;

                try {
                    modifier.RegisterProperties(driver);
                }
                catch (Exception err) {
                    // Prevents it from playing later, which helps ensure we don't let changes
                    // to the scene get saved accidentally.
                    behaviour.isFailed = true;
                    Debug.LogException(err);
                }
            }
        }

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
