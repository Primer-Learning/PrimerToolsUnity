using System;
using System.Collections.Generic;
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
            var found = director.FindTrack(
                track => track is T trackOfType && (director.GetGenericBinding(trackOfType) == boundTo)
            );

            return found as T ?? director.CreateTrack<T>(boundTo);
        }

        public static TrackAsset GetTrackOfClip(this PlayableDirector director, PlayableAsset target)
        {
            return director.FindTrack(track => track.GetClips().Any(clip => ReferenceEquals(clip.asset, target)));
        }

        public static TrackAsset FindTrack(this PlayableDirector director, Predicate<TrackAsset> predicate)
            => FindTrackRecursively(director.GetTimeline().GetRootTracks(), predicate);

        private static TrackAsset FindTrackRecursively(IEnumerable<TrackAsset> tracks, Predicate<TrackAsset> predicate)
        {
            foreach (var track in tracks) {
                if (predicate(track))
                    return track;

                if (track is not GroupTrack group)
                    continue;

                var found = FindTrackRecursively(group.GetChildTracks(), predicate);

                if (found)
                    return found;
            }

            return null;
        }

        private static TimelineAsset GetTimeline(this PlayableDirector director)
        {
            var timeline = director.playableAsset as TimelineAsset;

            if (timeline == null)
                throw new Exception("Can't find timeline");

            return timeline;
        }

    }
}
