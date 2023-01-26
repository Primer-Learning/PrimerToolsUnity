using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Animation.Editor
{
    [UsedImplicitly]
    [MenuEntry("Primer / Force cubic curves on all clips", priority: 9001)]
    public class ForceCubicAnimationCurve : TrackAction
    {
        public override ActionValidity Validate(IEnumerable<TrackAsset> tracks)
        {
            return ActionValidity.Valid;
        }

        public override bool Execute(IEnumerable<TrackAsset> tracks)
        {
            foreach (var track in tracks)
                ChangeAnimationCurves(track);

            return true;
        }

        private void ChangeAnimationCurves(TrackAsset track)
        {
            foreach (var subTrack in track.GetChildTracks())
                ChangeAnimationCurves(subTrack);

            foreach (var clip in track.GetClips())
                ChangeAnimationCurves(clip.animationClip);

            if (track is AnimationTrack aniTrack)
                ChangeAnimationCurves(aniTrack.infiniteClip);
        }

        protected virtual void ChangeAnimationCurves(AnimationClip clip)
            => clip.MakeCubicCurves();
    }
}
