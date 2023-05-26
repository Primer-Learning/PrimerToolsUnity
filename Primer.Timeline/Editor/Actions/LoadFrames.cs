using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEditor.Timeline.Actions;
using UnityEngine.Playables;

namespace Primer.Timeline.Editor
{
    [UsedImplicitly]
    [MenuEntry("Start loading frames", priority: 9005)]
    public class LoadFrames : TimelineAction
    {
        public static bool isPlaying = false;
        private const float STEP = 0.1f;

        public override ActionValidity Validate(ActionContext context)
        {
            return !isPlaying ? ActionValidity.Valid : ActionValidity.NotApplicable;
        }

        public override bool Execute(ActionContext context)
        {
            var director = TimelineEditor.inspectedDirector;
            isPlaying = true;
            StartPlaying(director);
            return true;
        }

        private static async void StartPlaying(PlayableDirector playableDirector)
        {
            while (isPlaying && playableDirector.time < playableDirector.duration) {
                await PlayModeControl.PlayDirectorAt(playableDirector, (float)playableDirector.time + STEP);
                await UniTask.Delay(1);
            }
        }
    }

    [UsedImplicitly]
    [MenuEntry("Stop loading frames", priority: 9005)]
    public class StopLoadingFrames : TimelineAction
    {
        public override ActionValidity Validate(ActionContext context)
        {
            return LoadFrames.isPlaying ? ActionValidity.Valid : ActionValidity.NotApplicable;
        }

        public override bool Execute(ActionContext context)
        {
            LoadFrames.isPlaying = false;
            return true;
        }
    }
}
