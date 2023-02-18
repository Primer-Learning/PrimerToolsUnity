using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [MenuEntry("Remove seconds...", priority: 9000)]
    public class RemoveSecondsFromTimeline : ManipulateTime
    {
        private const float DEFAULT_REMOVE_SECONDS = 1;


        public override bool Execute(ActionContext context)
        {
            var value = EditorInputDialog.Show(
                "Remove time",
                $"How many frames to remove from {inspectedTime}s?",
                DEFAULT_REMOVE_SECONDS
            );

            if (!value.HasValue)
                return false;

            Undo.RecordObject(TimelineEditor.inspectedAsset, "Remove time");
            RemoveTime(value.Value);
            return true;
        }


        public static void RemoveTime(float seconds)
        {
            var time = inspectedTime;

            foreach (var track in allTracks) {
                if (track is AnimationTrack animTrack && animTrack.infiniteClip != null) {
                    animTrack.infiniteClip.ModifyKeyFrames(
                        keyframe => {
                            if (keyframe.time > time)
                                keyframe.time -= Mathf.Min(seconds, keyframe.time - time);

                            return keyframe;
                        }
                    );
                }

                foreach (var clip in track.GetClips()) {
                    var toRemove = seconds;

                    if (clip.start > time) {
                        var moveBack = Mathf.Min((float)(clip.start - time), toRemove);
                        clip.start -= moveBack;
                        toRemove -= moveBack;
                    }

                    if (clip.end > time)
                        clip.duration -= Mathf.Min((float)(clip.duration - (time - clip.start)), toRemove);

                    if (clip.duration == 0)
                        track.DeleteClip(clip);
                }
            }
        }
    }
}
