using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [MenuEntry("Remove seconds...", priority: 9010)]
    public class RemoveSecondsFromTimeline : TimelineAction
    {
        private const float DEFAULT_REMOVE_SECONDS = 1;

        public override ActionValidity Validate(ActionContext context) => ActionValidity.Valid;

        public override bool Execute(ActionContext context)
        {
            var time = (float)TimelineEditor.inspectedDirector.time;
            var timeline = TimelineEditor.inspectedAsset;
            var defaultValue = Mathf.Min(DEFAULT_REMOVE_SECONDS, timeline.GetMaxRemovableTimeAt(time));

            var value = EditTimelineDialog.Show(
                "Remove time",
                $"How many seconds to remove from {time}s?",
                defaultValue,
                time + defaultValue
            );

            if (value is null)
                return false;

            timeline.RemoveTime(time, value.seconds, value.preserveClips);
            return true;
        }
    }
}
