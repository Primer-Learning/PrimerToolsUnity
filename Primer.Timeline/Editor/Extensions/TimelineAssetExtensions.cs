using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    public static class TimelineAssetExtensions
    {
        public static void AddTime(this TimelineAsset timeline, float time, float seconds, bool preserveClips = false)
        {
            var allTracks = timeline.GetAllTracks();

            if (preserveClips && allTracks.SelectMany(track => track.GetClips()).Any(clip => clip.start < time && clip.end > time)) {
                throw new Exception("Cannot add time in the middle of a clip when preserveClips is true");
            }

            foreach (var track in allTracks) {
                if (track is AnimationTrack animTrack && animTrack.infiniteClip != null) {
                    animTrack.infiniteClip.ModifyKeyframes(
                        keyframe => {
                            if (keyframe.time >= time)
                                keyframe.time += seconds;

                            return keyframe;
                        }
                    );
                }

                foreach (var clip in track.GetClips()) {
                    if (clip.start > time)
                        clip.start += seconds;
                    else if (clip.end > time)
                        clip.duration += seconds;
                }
            }
        }


        public static void RemoveTime(this TimelineAsset timeline, float time, float seconds, bool preserveClips = false)
        {
            var totalTimeToRemove = preserveClips
                ? Mathf.Min(seconds, GetMaxRemovableTime(timeline, time))
                : seconds;

            if (preserveClips && totalTimeToRemove != seconds) {
                Debug.LogWarning(
                    $"Asked to remove {seconds}s from the timeline while preserving clips, next clip is at {time + totalTimeToRemove}s, removing {totalTimeToRemove}s instead"
                );
            }

            foreach (var track in timeline.GetAllTracks()) {
                if (track is AnimationTrack animTrack && animTrack.infiniteClip != null) {
                    animTrack.infiniteClip.ModifyKeyframes(
                        keyframe => {
                            if (keyframe.time > time)
                                keyframe.time -= Mathf.Min(totalTimeToRemove, keyframe.time - time);

                            return keyframe;
                        }
                    );
                }

                foreach (var clip in track.GetClips()) {
                    var toRemove = totalTimeToRemove;

                    if (clip.start > time) {
                        var moveBack = Mathf.Min((float)(clip.start - time), toRemove);
                        clip.start -= moveBack;
                        toRemove -= moveBack;
                    }

                    if (preserveClips)
                        continue;

                    if (clip.end > time)
                        clip.duration -= Mathf.Min((float)(clip.duration - (time - clip.start)), toRemove);

                    if (clip.duration == 0)
                        track.DeleteClip(clip);
                }
            }
        }

        private static float GetMaxRemovableTime(TimelineAsset timeline, float time)
        {
            var max = float.MaxValue;

            foreach (var track in timeline.GetAllTracks()) {
                if (track is AnimationTrack animTrack && animTrack.infiniteClip != null) {
                    // Get the minimum distance between the time we want to remove and the next keyframe
                    max = animTrack.infiniteClip.IterateAllKeyframes()
                        .SelectMany(x => x)
                        .Where(keyframe => keyframe.time > time)
                        .Aggregate(
                            max,
                            (current, keyframe) => Mathf.Min(current, keyframe.time - time)
                        );
                }

                foreach (var clip in track.GetClips()) {
                    if (clip.start > time)
                        max = Mathf.Min(max, (float)(clip.start - time));
                    else if (clip.end > time)
                        max = 0;
                }
            }

            return max;
        }


        public static List<TrackAsset> GetAllTracks(this TimelineAsset timeline)
        {
            return GetAllTracksRecursively(timeline.GetRootTracks());
        }

        private static List<TrackAsset> GetAllTracksRecursively(IEnumerable<TrackAsset> tracks)
        {
            var result = new List<TrackAsset>();

            foreach (var track in tracks) {
                if (track is GroupTrack group)
                    result.AddRange(GetAllTracksRecursively(group.GetChildTracks()));
                else
                    result.Add(track);
            }

            return result;
        }
    }
}
