using System;
using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [MenuEntry("Add seconds...", priority: 9000)]
    public class AddSecondsToTimeline : TimelineAction
    {
        private const float DEFAULT_ADD_SECONDS = 1;

        public override ActionValidity Validate(ActionContext context) => ActionValidity.Valid;

        public override bool Execute(ActionContext context)
        {
            var time = (float)TimelineEditor.inspectedDirector.time;
            var timeline = TimelineEditor.inspectedAsset;

            var value = EditTimelineDialog.Show(
                "Add time",
                $"How many seconds to add after {time}s?",
                DEFAULT_ADD_SECONDS
            );

            if (value is null)
                return false;

            if (value.preserveClips && timeline.HasSomeClipAt(time))
                throw new Exception("Cannot add time in the middle of a clip when preserveClips is true");

            timeline.AddTime(time, value.seconds, value.preserveClips);
            return true;
        }
    }
}
