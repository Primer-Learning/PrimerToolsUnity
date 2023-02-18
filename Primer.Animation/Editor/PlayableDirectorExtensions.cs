using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
    }
}
