using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Primer.Animation
{
    public static class PlayableDirectorExtensions
    {
        public static void CreateAnimation(this PlayableDirector director, GameObject target,
            IPrimerAnimation animation)
        {
            var animator = target.GetOrAddComponent<Animator>();
            var track = director.GetOrCreateTrack<AnimationTrack>(animator);

            if (track.infiniteClip == null) {
                track.CreateInfiniteClip(animation.name);
                track.infiniteClipPreExtrapolation = TimelineClip.ClipExtrapolation.Hold;
                track.infiniteClipPostExtrapolation = TimelineClip.ClipExtrapolation.Hold;
            }

            animation.ApplyTo(track.infiniteClip);
        }

        public static T CreateTrack<T>(this PlayableDirector director, Object boundTo) where T : TrackAsset, new()
        {
            var timeline = director.GetTimeline();
            var track = timeline.CreateTrack<T>();

            director.SetGenericBinding(track, boundTo);

            return track;
        }

        public static object GetGenericBindingForClip(this PlayableDirector director, PlayableAsset clip)
        {
            var track = director.GetTrackOfClip(clip);
            return track is null ? null : director.GetGenericBinding(track);
        }

        public static T GetOrCreateTrack<T>(this PlayableDirector director, Object boundTo) where T : TrackAsset, new()
        {
            var timeline = director.GetTimeline();

            foreach (var track in timeline.GetRootTracks()) {
                if (track is T trackOfType && (director.GetGenericBinding(trackOfType) == boundTo)) {
                    return trackOfType;
                }
            }

            return director.CreateTrack<T>(boundTo);
        }

        public static TimelineAsset GetTimeline(this PlayableDirector director)
        {
            var timeline = director.playableAsset as TimelineAsset;

            if (timeline == null)
                throw new Exception("Can't find timeline");

            return timeline;
        }

        public static TrackAsset GetTrackOfClip(this PlayableDirector director, PlayableAsset target)
        {
            var timeline = director.GetTimeline();

            foreach (var track in timeline.GetRootTracks()) {
                if (track.GetClips().Any(clip => ReferenceEquals(clip.asset, target)))
                    return track;
            }

            return null;
        }
    }
}
