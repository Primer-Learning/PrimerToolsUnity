using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Primer.Timeline.Editor
{
    public static class TimelineAssetExtensions
    {
        public static void AddTime(this TimelineAsset timeline, float time, float seconds, bool preserveClips = false)
        {
            if (preserveClips && timeline.HasUnlockedClipAt(time))
                throw new Exception("Cannot add time in the middle of a clip when preserveClips is true");

            var undoMessage = $"Add {seconds}s at {time}s";
            var tracks = timeline.GetAllUnlockedTracks();
            var clips = tracks.SelectMany(x => x.GetClips()).ToList();
            var anim = tracks.OfType<AnimationTrack>()
                .Where(x => x.infiniteClip != null)
                .Select(x => x.infiniteClip)
                .ToList();

            UndoExtensions.RegisterClips(clips, undoMessage);

            foreach (var clip in clips) {
                if ((float)clip.start >= time)
                    clip.start += seconds;
                else if ((float)clip.end > time)
                    clip.duration += seconds;
            }

            Undo.RecordObjects(anim.Cast<Object>().ToArray(), undoMessage);

            foreach (var clip in anim) {
                clip.ModifyKeyframes(keyframe => {
                    if (keyframe.time >= time)
                        keyframe.time += seconds;

                    return keyframe;
                });
            }
            
            // Refresh the PlayableDirector associated with the TimelineAsset
            RefreshPlayableDirector(timeline);
        }

        public static bool HasUnlockedClipAt(this TimelineAsset timeline, float time)
        {
            return timeline.GetAllUnlockedTracks()
                .SelectMany(track => track.GetClips())
                .Any(clip => (float)clip.start < time && (float)clip.end > time);
        }


        public static void RemoveTime(this TimelineAsset timeline, float time, float seconds, bool preserveClips = false)
        {
            var totalTimeToRemove = preserveClips
                ? Mathf.Min(seconds, timeline.GetMaxRemovableTimeAt(time))
                : seconds;

            if (preserveClips && totalTimeToRemove != seconds) {
                Debug.LogWarning(
                    $"Asked to remove {seconds}s from the timeline while preserving clips, next clip is at {time + totalTimeToRemove}s, removing {totalTimeToRemove}s instead"
                );
            }

            var undoMessage = $"Remove {seconds}s at {time}s";
            var tracks = timeline.GetAllUnlockedTracks();
            var clips = tracks.SelectMany(x => x.GetClips()).ToList();
            var anim = tracks.OfType<AnimationTrack>()
                .Where(x => x.infiniteClip != null)
                .Select(x => x.infiniteClip)
                .ToList();

            UndoExtensions.RegisterClips(clips, undoMessage);

            foreach (var clip in clips) {
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
                    clip.GetParentTrack().DeleteClip(clip);
            }

            Undo.RecordObjects(anim.Cast<Object>().ToArray(), undoMessage);

            foreach (var clip in anim) {
                clip.ModifyKeyframes(keyframe => {
                    if (keyframe.time > time)
                        keyframe.time -= Mathf.Min(totalTimeToRemove, keyframe.time - time);

                    return keyframe;
                });
            }
            
            // Refresh the PlayableDirector associated with the TimelineAsset
            RefreshPlayableDirector(timeline);
        }

        public static float GetMaxRemovableTimeAt(this TimelineAsset timeline, float time)
        {
            var max = float.MaxValue;

            foreach (var track in timeline.GetAllUnlockedTracks()) {
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


        public static List<TrackAsset> GetAllUnlockedTracks(this TimelineAsset timeline)
        {
            return GetAllTracksRecursively(timeline.GetRootTracks()).Where(x => !x.locked).ToList();
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
        // Method is from GPT4. Seems to work!
        private static void RefreshPlayableDirector(TimelineAsset timeline)
        {
            // Find and refresh the PlayableDirector component in the scene
            PlayableDirector[] directors = GameObject.FindObjectsOfType<PlayableDirector>();

            foreach (var director in directors)
            {
                if (director.playableAsset == timeline)
                {
                    director.RebuildGraph();
                    director.Evaluate(); // This line may not be necessary but can help to force the timeline evaluation
                    break;
                }
            }
        }
    }
}
