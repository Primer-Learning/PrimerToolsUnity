using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Timeline
{
    /// <summary>
    /// This class exists because the timeline doesn't render the objects properly on the first run
    /// </summary>
    [InitializeOnLoad]
    internal static class FixTimelineInPlayMode
    {
        public static bool isPreloading = false;
        public static bool isPlaying = false;
        private static Action whenReady;

        static FixTimelineInPlayMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        public static void OnEnterPlayMode(Action listener) {
            whenReady += listener;
        }

        private static async void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode) {
                isPlaying = false;
                return;
            }

            var director = Object.FindObjectOfType<PlayableDirector>();
            if (director) {
                await HandlePlayMode(director);
            }

            isPlaying = true;
            whenReady?.Invoke();
            whenReady = null;
        }

        private static async Task HandlePlayMode(PlayableDirector director)
        {
            director.Pause();
            isPreloading = true;

            await PrimerTimeline.ScrubTo(director, director.duration);
            await PrimerTimeline.ScrubTo(director, 0);

            // A little more time to let things settle before playing.
            await UniTask.Delay(100);

            isPreloading = false;
            director.Play();
        }
    }
}
