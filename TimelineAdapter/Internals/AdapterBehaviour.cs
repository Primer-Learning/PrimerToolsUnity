using UnityEngine.Playables;

namespace Primer.Timeline
{
    internal class AdapterBehaviour : PlayableBehaviour
    {
        public ScrubbableAdapter adapter;

        /// <summary>Duration of the clip, not considering any extrapolation.</summary>
        /// <remarks>
        ///     <tt>playable.GetDuration()</tt> includes the extrapolated time as well, so may be
        ///     infinity.
        /// </remarks>
        public double duration;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            var args = new ScrubbableAdapter.UpdateArgs {
                time = playable.GetTime() / duration
            };

            adapter.Update(args);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            adapter.Cleanup();
        }

        public override void OnGraphStart(Playable playable) {
            adapter.Prepare();
        }
    }
}
