using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Animation
{
    public static class PlayableDirectorExtensions
    {
        public static TimelineAsset GetTimeline(this PlayableDirector director)
        {
            var timeline = director.playableAsset as TimelineAsset;
            if (timeline == null) throw new System.Exception("Can't find timeline");
            return timeline;
        }

        public static T CreateTrack<T>(this PlayableDirector director, Object boundTo) where T : TrackAsset, new()
        {
            var timeline = director.GetTimeline();
            var track = timeline.CreateTrack<T>();

            director.SetGenericBinding(track, boundTo);

            return track;
        }

        public static T GetOrCreateTrack<T>(this PlayableDirector director, Object boundTo) where T : TrackAsset, new()
        {
            var timeline = director.GetTimeline();

            foreach (var track in timeline.GetRootTracks()) {
                if (track is T trackOfType && director.GetGenericBinding(trackOfType) == boundTo) {
                    return trackOfType;
                }
            }

            return director.CreateTrack<T>(boundTo);
        }

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
    }
}
