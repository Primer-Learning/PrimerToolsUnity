using JetBrains.Annotations;
using UnityEditor;
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

            var value = EditorInputDialog.Show(
                "Add time",
                $"How many frames to add after {time}s?",
                DEFAULT_ADD_SECONDS
            );

            if (!value.HasValue)
                return false;

            Undo.RecordObject(TimelineEditor.inspectedAsset, "Add time");
            TimelineEditor.inspectedAsset.AddTime(time, value.Value, true);
            return true;
        }
    }
}
