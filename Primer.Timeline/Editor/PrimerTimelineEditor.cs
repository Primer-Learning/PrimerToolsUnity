using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline.Editor
{
    public static class PrimerTimelineEditor
    {
        private const float DEFAULT_ADD_SECONDS = 1;
        private const float DEFAULT_REMOVE_SECONDS = 1;

        private static float time => (float)TimelineEditor.inspectedDirector.time;
        private static IEnumerable<TrackAsset> allTracks => TimelineEditor.inspectedDirector.GetAllTracks();


        [UsedImplicitly]
        [MenuEntry("Primer / Add seconds...", priority: 9001)]
        public class AddSeconds : TimelineAction
        {
            public override ActionValidity Validate(ActionContext context) => ActionValidity.Valid;

            public override bool Execute(ActionContext context)
            {
                var value = EditorInputDialog.Show(
                    "Add time",
                    $"How many frames to add after {time}s?",
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
                foreach (var clip in allTracks.SelectMany(track => track.GetClips())) {
                    if (clip.start >= time)
                        clip.start += seconds;
                    else if (clip.end >= time)
                        clip.duration += seconds;
                }
            }
        }


        [UsedImplicitly]
        [MenuEntry("Primer / Remove seconds...", priority: 9001)]
        public class RemoveSeconds : TimelineAction
        {
            public override ActionValidity Validate(ActionContext context) => ActionValidity.Valid;

            public override bool Execute(ActionContext context)
            {
                var value = EditorInputDialog.Show(
                    "Remove time",
                    $"How many frames to remove from {time}s?",
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
                foreach (var track in allTracks) {
                    foreach (var clip in track.GetClips()) {
                        var toRemove = seconds;

                        if (clip.start > time) {
                            var moveBack = Mathf.Min((float)(clip.start - time), toRemove);
                            clip.start -= moveBack;
                            toRemove -= moveBack;
                        }

                        if (clip.end > time) {
                            var crop = Mathf.Min((float)(clip.duration - (time - clip.start)), toRemove);

                            if (crop == clip.duration)
                                track.DeleteClip(clip);
                            else
                                clip.duration -= crop;

                            // BEHOLD! the closing brackets of doom!
                        } // if
                    } // foreach clip
                } // foreach track
            } // method
        } // inner class
    } // class
} // namespace
