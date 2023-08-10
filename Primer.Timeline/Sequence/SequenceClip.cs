using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    internal class SequenceClip : PrimerClip
    {
        protected override PrimerPlayable template { get; } = new SequencePlayable();

        public override string clipName => $"[{template.clipIndex + 1}] {trackTransform?.name ?? "No sequence"}";

#if UNITY_EDITOR
        // When we press one of these "View in play mode" buttons the C# scripts get re-compiled
        //  any running function gets interrupted and awaits are cancelled.
        //
        // This is why we need to save (serialize) the `connector` to resume the function after the re-compile.
        [SerializeReference, HideInInspector]
        private ViewInPlayModeBehaviour connector = null;

        private void OnEnable()
        {
            if (connector is not null) {
                PatchPlayMode.OnEnterPlayMode(connector.Execute);
                connector = null;
            }
        }

        [Title("View in play mode...")]
        [PropertySpace(32)]

        [Button]
        [DetailedInfoBox("Slow and reliable", "Enter play mode at regular speed and pause before this clip starts.")]
        private void AndPauseBefore_Starter(float secondsBefore = 1)
        {
            connector = new PlayAndPauseBefore(start - secondsBefore);
            EnterPlayMode();
        }

        [Button]
        [PropertySpace(16)]
        [DetailedInfoBox(
            "Midpoint",
            "Enter play mode and run all frames at max speed until before this clip starts.\nIf frameRate is 0, Time.captureFramerate is used."
        )]
        private void AndFastForward_Starter(float secondsBefore = 2, float frameRate = 60)
        {
            connector = new PlayAndFastForward(start - secondsBefore, frameRate);
            EnterPlayMode();
        }

        [Button]
        [PropertySpace(16)]
        [DetailedInfoBox("Fast and unreliable", "Enter play mode and scrub to the start of this clip.")]
        private void AndScrub_Starter(float secondsBefore = 2)
        {
            connector = new PlayAndScrub(start - secondsBefore);
            EnterPlayMode();
        }

        private static void EnterPlayMode()
        {
            if (!Application.isPlaying && ViewInPlayModeBehaviour.FindDirector() is not null)
                PrimerTimeline.EnterPlayMode();
        }
#endif
    }
}
