using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerPlayable : PlayableBehaviour
    {
        private bool isInitialized;

        protected virtual void Start() {}

        protected virtual void Stop() {}


        protected void RunStart()
        {
            if (isInitialized)
                return;

            Start();
            isInitialized = true;
        }

        protected void RunStop()
        {
            if (!isInitialized)
                return;

            Stop();
            isInitialized = false;
        }


        #region Unity playable overrides
        public override void OnBehaviourPause(Playable playable, FrameData info) => RunStop();

        public override void OnPlayableDestroy(Playable playable) => RunStop();

        public override void OnGraphStop(Playable playable) => RunStop();
        #endregion
    }
}
