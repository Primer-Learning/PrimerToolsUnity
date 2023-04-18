using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerMixer : PlayableBehaviour
    {
        public bool isMuted;

        public override void OnPlayableDestroy(Playable playable)
        {
            PrimerTimeline.DisposeEphemeralObjects();
        }
    }
}
