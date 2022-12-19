using Primer.Editor;
using UnityEditor;
using UnityEditor.Timeline;

namespace Primer.Animation.Editor
{
    public static class AnimationCreateUtility
    {
        [MenuItem("GameObject/Primer animation/Scale up from zero", true, CreateUtility.PRIORITY)]
        [MenuItem("GameObject/Primer animation/Scale down to zero", true, CreateUtility.PRIORITY)]
        public static bool ScaleUpFromZero_Validate(MenuCommand command) =>
            command.HasSelectedElement();

        [MenuItem("GameObject/Primer animation/Scale up from zero", false, CreateUtility.PRIORITY)]
        public static void ScaleUpFromZero(MenuCommand command)
        {
            var director = TimelineEditor.inspectedDirector;
            var selected = command.GetSelectedElement();
            var animation = new ScaleUpFromZero(selected.transform.localScale, director.time);

            director.CreateAnimation(selected, animation);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }

        [MenuItem("GameObject/Primer animation/Scale down to zero", false, CreateUtility.PRIORITY)]
        public static void ScaleDownToZero(MenuCommand command)
        {
            var director = TimelineEditor.inspectedDirector;
            var selected = command.GetSelectedElement();
            var animation = new ScaleDownToZero(selected.transform.localScale, director.time);

            director.CreateAnimation(selected, animation);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }
}
