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
        // When we press one of these buttons the C# scripts get re-compiled
        //  any running function gets interrupted and awaits are cancelled.
        // This is why we need to use the `connector` enum to resume the function
        //  after the re-compile.
        // And we also save the parameters for the functions
        //  this is ugly and painful but it does the job.

        [SerializeField]
        private ViewInPlayModeConnector connector = ViewInPlayModeConnector.None;
        [SerializeField]
        private float secondsBeforeArg;
        [SerializeField]
        private float frameRateArg;

        private enum ViewInPlayModeConnector
        {
            None,
            AndPauseBefore,
            AndFastForward,
            AndScrub,
        }

        private void OnEnable()
        {
            if (connector is ViewInPlayModeConnector.None)
                return;

            if (connector == ViewInPlayModeConnector.AndPauseBefore)
                AndPauseBefore(secondsBeforeArg);
            else if (connector == ViewInPlayModeConnector.AndFastForward)
                AndFastForward(secondsBeforeArg, frameRateArg);
            else if (connector == ViewInPlayModeConnector.AndScrub)
                AndScrub(secondsBeforeArg);

            connector = ViewInPlayModeConnector.None;
        }

        private static PlayableDirector foundDirector => FindObjectOfType<PlayableDirector>();

        [Title("View in play mode...")]
        [PropertySpace(32)]

        [Button]
        [DetailedInfoBox("Slow and reliable", "Enter play mode at regular speed and pause before this clip starts.")]
        private void AndPauseBefore_Starter(float secondsBefore = 1)
        {
            connector = ViewInPlayModeConnector.AndPauseBefore;
            secondsBeforeArg = secondsBefore;
            EnterPlayMode();
        }

        private async void AndPauseBefore(float secondsBefore)
        {
            var director = foundDirector;

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
        private void AndFastForward_Starter(float secondsBefore = 2, float frameRate = 60)
        {
            connector = ViewInPlayModeConnector.AndFastForward;
            secondsBeforeArg = secondsBefore;
            frameRateArg = frameRate;
            EnterPlayMode();
        }

        private async void AndFastForward(float secondsBefore, float frameRate)
        {
            var fps = frameRate > 0 ? frameRate : Time.captureFramerate;

            if (fps <= 0)
                throw new Exception("Can't fast forward because frameRate is 0 and Time.captureFramerate is 0.");

            var director = foundDirector;

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
        private void AndScrub_Starter(float secondsBefore = 2)
        {
            connector = ViewInPlayModeConnector.AndScrub;
            secondsBeforeArg = secondsBefore;
            EnterPlayMode();
        }

        private async void AndScrub(float secondsBefore)
        {
            var director = foundDirector;

            if (director is not null)
                await PrimerTimeline.ScrubTo(director, start - secondsBefore);
        }

        private static void EnterPlayMode()
        {
            if (!Application.isPlaying && foundDirector is not null)
                PrimerTimeline.EnterPlayMode();
        }
#endif
        #endregion
    }
}
