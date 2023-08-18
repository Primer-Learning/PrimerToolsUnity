using UnityEngine.Playables;

namespace Primer.Timeline
{
    /// <summary>
    ///   Each track can define a Mixer that defines how are the clips combined.
    ///   This is meant to be the base class for all of them.
    /// </summary>
    public class PrimerMixer : PlayableBehaviour
    {
        public bool isMuted;
    }
}
