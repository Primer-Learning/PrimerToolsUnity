using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    [DisplayName("Primer Learning/Adapter Track")]
    [TrackClipType(typeof(AdapterClip))]
    internal class AdapterTrack : TrackAsset
    {
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver) {
            base.GatherProperties(director, driver);

            PropertyRegistrar registrar = new(driver);
            foreach (var clip in GetClips()) {
                var adapterClip = (AdapterClip)clip.asset;
                if (adapterClip.adapter is not {} adapter) continue;
                try {
                    adapter.resolver = director;
                    adapter.RegisterPreviewingProperties(registrar);
                }
                catch (Exception err) {
                    // Adding the error to the adapter prevents it from playing later, which helps
                    // ensure we don't let changes to the scene get saved accidentally.
                    adapter.errors.Add(err);

                    Debug.LogException(err);
                }
            }
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject,
            TimelineClip clip) {
            var playable = base.CreatePlayable(graph, gameObject, clip);

            // Only the TimelineClip has the actual duration (not including extrapolation) so we
            // must grab it here.
            var behaviour = ((ScriptPlayable<AdapterBehaviour>)playable).GetBehaviour();
            if (behaviour is not null) {
                behaviour.duration = clip.duration;
            }

            return playable;
        }
    }
}
