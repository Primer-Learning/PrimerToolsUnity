using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [MenuEntry("Add seconds...", priority: 9000)]
    public class AddSecondsToTimeline : ManipulateTime
    {
        private const float DEFAULT_ADD_SECONDS = 1;


        public override bool Execute(ActionContext context)
        {
            var value = EditorInputDialog.Show(
                "Add time",
                $"How many frames to add after {inspectedTime}s?",
                DEFAULT_ADD_SECONDS
            );

            if (!value.HasValue)
                return false;

            Undo.RecordObject(TimelineEditor.inspectedAsset, "Add time");
            AddTime(value.Value);
            return true;
        }


        public static void AddTime(float seconds)
        {
            var time = inspectedTime;

            foreach (var track in allTracks){
                if (track is AnimationTrack animTrack && animTrack.infiniteClip != null) {
                    animTrack.infiniteClip.ModifyKeyFrames(
                        keyframe => {
                            if (keyframe.time >= time)
                                keyframe.time += seconds;

                            return keyframe;
                        }
                    );
                }

                foreach (var clip in track.GetClips()) {
                    if (clip.start >= time)
                        clip.start += seconds;
                    else if (clip.end >= time)
                        clip.duration += seconds;
                }
            }
        }
    }
}
