using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
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
        private const int INITIAL_DELAY = 100;
        public static bool isPlaying = false;

        static FixTimelineInPlayMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
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

            await UniTask.Delay(INITIAL_DELAY);
            isPlaying = true;
        }

        private static async Task HandlePlayMode(PlayableDirector director)
        {
            director.Pause();
            await PrimerTimeline.ScrubTo(director, director.duration);
            await PrimerTimeline.ScrubTo(director, 0);
            director.Play();
        }
    }
}
