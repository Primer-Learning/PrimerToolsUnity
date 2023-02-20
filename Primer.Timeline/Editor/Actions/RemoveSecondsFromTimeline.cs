using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;

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

            var value = EditorInputDialog.Show(
                "Remove time",
                $"How many frames to remove from {time}s?",
                DEFAULT_REMOVE_SECONDS
            );

            if (!value.HasValue)
                return false;

            Undo.RecordObject(TimelineEditor.inspectedAsset, "Remove time");
            TimelineEditor.inspectedAsset.RemoveTime(time, value.Value, true);
            return true;
        }
    }
}
