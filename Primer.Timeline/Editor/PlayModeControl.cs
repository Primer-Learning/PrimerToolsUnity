using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline.Editor
{
    /// <summary>
    /// This class exists because the timeline doesn't render the objects properly on the first run
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeControl
    {
        // private static float startAt = 0;

        static PlayModeControl()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        // public static void RunAt(float time)
        // {
        //     startAt = Mathf.Max(time, 0);
        //     EditorApplication.EnterPlaymode();
        // }

        private static async void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
                await FixTimelineInPlayMode();
        }

        private static async Task FixTimelineInPlayMode()
        {
            var director = Object.FindObjectOfType<PlayableDirector>();
            director.Pause();

            await PlayDirectorAt(director, SequenceOrchestrator.end);

            // for (var i = 0f; i < startAt; i += 0.1f) {
            //     await PlayDirectorAt(director, i);
            //     await UniTask.Delay(1);
            // }
            //
            // await PlayDirectorAt(director, startAt);
            // startAt = 0;

            await PlayDirectorAt(director, 0);

            // director.Play();
            // EditorApplication.isPaused = true;
        }

        public static async UniTask PlayDirectorAt(PlayableDirector director, float time)
        {
            director.time = time;
            director.Evaluate();

            await SequenceOrchestrator.AllSequencesFinished();
            await PrimerTimeline.AllAsynchronyFinished();
        }
    }
}
