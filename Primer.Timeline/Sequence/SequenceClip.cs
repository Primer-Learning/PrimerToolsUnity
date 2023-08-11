using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    internal class SequenceClip : PrimerClip
    {
        protected override PrimerPlayable template { get; } = new SequencePlayable();

        public override string clipName => $"[{template.clipIndex + 1}] {trackTransform?.name ?? "No sequence"}";

#if UNITY_EDITOR
        [Title("View in play mode...")]
        [PropertySpace(32)]

        [Button]
        [DetailedInfoBox("Slow and reliable", "Enter play mode at regular speed and pause before this clip starts.")]
        private void AndPauseBefore(float secondsBefore = 1)
        {
            var component = new GameObject().AddComponent<PlayAndPauseBefore>();
            component.name = "Pause before clip";
            component.at = start - secondsBefore;
            component.director = FindDirector();
            component.EnterPlayMode();
        }

        [Button]
        [PropertySpace(16)]
        [DetailedInfoBox(
            "Midpoint",
            "Enter play mode and run all frames at max speed until before this clip starts.\nIf frameRate is 0, Time.captureFramerate is used."
        )]
        private void AndFastForward(float secondsBefore = 2, float frameRate = 60)
        {
            var component = new GameObject().AddComponent<PlayAndFastForward>();
            component.name = "Pause before clip";
            component.to = start - secondsBefore;
            component.frameRate = frameRate;
            component.director = FindDirector();
            component.EnterPlayMode();
        }

        [Button]
        [PropertySpace(16)]
        [DetailedInfoBox("Fast and unreliable", "Enter play mode and scrub to the start of this clip.")]
        private void AndScrub(float secondsBefore = 2)
        {
            var component = new GameObject().AddComponent<PlayAndScrub>();
            component.name = "Pause before clip";
            component.to = start - secondsBefore;
            component.director = FindDirector();
            component.EnterPlayMode();
        }

        private static PlayableDirector FindDirector()
        {
            return FindObjectOfType<PlayableDirector>();
        }
#endif
    }
}
