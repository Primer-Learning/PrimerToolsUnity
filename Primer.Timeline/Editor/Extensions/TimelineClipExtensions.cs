using System;
using UnityEditor.Timeline;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    public static class TimelineClipExtensions
    {
        public static void SetClipDuration(this PlayableAsset asset, float clipAssetExpectedDuration)
        {
            var clips = TimelineEditor.inspectedDirector.GetClipsInTrack(asset);
            var index = Array.FindIndex(clips, c => c.asset == asset);
            clips.IncreaseClipDuration(index, clipAssetExpectedDuration);
        }

        public static void IncreaseClipDuration(this TimelineClip[] clips, int i, float expectedDuration)
        {
            var targetClip = clips[i];
            var neededSpace = expectedDuration - targetClip.duration;

            if (i == clips.Length - 1) {
                targetClip.duration = expectedDuration;
                return;
            }

            var nextClip = clips[i + 1];
            var availableSpace = nextClip.start - targetClip.end;

            if (availableSpace < neededSpace)
                MoveNextClips(clips, i + 1, neededSpace - availableSpace);

            targetClip.duration += neededSpace;
        }

        private static void MoveNextClips(this TimelineClip[] clips, int i, double neededSpace)
        {
            if (i == clips.Length - 1) {
                clips[i].start += neededSpace;
                return;
            }

            var targetClip = clips[i];
            var nextClip = clips[i + 1];
            var availableSpace = nextClip.start - targetClip.end;

            if (neededSpace > availableSpace)
                MoveNextClips(clips, i + 1, neededSpace - availableSpace);

            targetClip.start += neededSpace;
        }
    }
}
