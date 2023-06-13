using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    internal class SequenceClip : PrimerClip
    {
        protected override PrimerPlayable template { get; } = new SequencePlayable();

        public override string clipName => $"[{template.clipIndex + 1}] {trackTransform?.name ?? "No sequence"}";


        #region Editor buttons
#if UNITY_EDITOR
        [Title("View in play mode...")]
        [PropertySpace(32)]

        [Button]
        [DetailedInfoBox("Slow and reliable", "Enter play mode at regular speed and pause before this clip starts.")]
        private async void AndPauseBefore(float secondsBefore = 1)
        {
            var director = await EnterPlayMode();

            if (director is null)
                return;

            var pauseAfter = start - secondsBefore;
            PatchPlayMode.isPlaying = false;
            await UniTask.WaitWhile(() => director.time < pauseAfter);
            EditorApplication.isPaused = true;
            PatchPlayMode.isPlaying = true;
        }

        [Button]
        [PropertySpace(16)]
        [DetailedInfoBox(
            "Midpoint",
            "Enter play mode and run all frames at max speed until before this clip starts.\nIf frameRate is 0, Time.captureFramerate is used."
        )]
        private async void AndFastForward(float secondsBefore = 2, float frameRate = 60)
        {
            var fps = frameRate > 0 ? frameRate : Time.captureFramerate;

            if (fps <= 0)
                throw new Exception("Can't fast forward because frameRate is 0 and Time.captureFramerate is 0.");

            var director = await EnterPlayMode();

            if (director is null)
                return;

            var secondsPerFrame = 1f / fps;
            var pauseAfter = start - secondsBefore;

            director.Pause();
            PatchPlayMode.isPlaying = false;

            while (director.time < pauseAfter) {
                await PrimerTimeline.ScrubTo(director, director.time + secondsPerFrame);
            }

            // cautionary wait to let the director catch up
            await UniTask.Delay(100);

            PatchPlayMode.isPlaying = true;
            director.Play();
        }

        [Button]
        [PropertySpace(16)]
        [DetailedInfoBox("Fast and unreliable", "Enter play mode and scrub to the start of this clip.")]
        private async void AndScrub(float secondsBefore = 2)
        {
            var director = await EnterPlayMode();

            if (director is null)
                return;

            await PrimerTimeline.ScrubTo(director, start - secondsBefore);
        }

        private static async UniTask<PlayableDirector> EnterPlayMode()
        {
            var director = FindObjectOfType<PlayableDirector>();

            if (Application.isPlaying || director == null)
                return null;

            await PrimerTimeline.EnterPlayMode();
            return director;
        }
#endif
        #endregion
    }
}
