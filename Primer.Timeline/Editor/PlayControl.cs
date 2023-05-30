using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Timeline.Editor
{
    /// <summary>
    /// This class exists because the timeline doesn't render the objects properly on the first run
    /// </summary>
    [InitializeOnLoad]
    public static class PlayControl
    {
        static PlayControl()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChange;
        }

        private static async void OnPlayModeStateChange(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
                return;

            var director = Object.FindObjectOfType<PlayableDirector>();
            if (director) {
                await FixTimelineInPlayMode(director);
            }
        }

        internal static async Task FixTimelineInPlayMode(PlayableDirector director)
        {
            director.Pause();
            await PrimerTimeline.ScrubTo(director, director.duration);
            await PrimerTimeline.ScrubTo(director, 0);
            director.Play();
        }
    }
}
