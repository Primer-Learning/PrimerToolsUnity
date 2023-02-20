using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [MenuEntry("Remove seconds...", priority: 9000)]
    public class RemoveSecondsFromTimeline : TimelineAction
    {
        private const float DEFAULT_REMOVE_SECONDS = 1;

        public override ActionValidity Validate(ActionContext context) => ActionValidity.Valid;

        public override bool Execute(ActionContext context)
        {
            var time = (float)TimelineEditor.inspectedDirector.time;
            var timeline = TimelineEditor.inspectedAsset;

            var value = EditTimelineDialog.Show(
                "Remove time",
                $"How many seconds to remove from {time}s?",
                Mathf.Min(DEFAULT_REMOVE_SECONDS, timeline.GetMaxRemovableTimeAt(time))
            );

            if (!value.HasValue)
                return false;

            Undo.RecordObject(timeline, "Remove time");
            timeline.RemoveTime(time, value.Value, true);
            return true;
        }
    }
}
