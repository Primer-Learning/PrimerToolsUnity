using UnityEngine.Playables;

namespace Primer.Timeline
{
    /// <summary>
    ///     Any class extending this one should call <pre>RunStart()</pre> before any frame operation.
    ///     Otherwise <pre>Start()</pre> won't be invoked.
    /// </summary>
    public class PrimerPlayable : PlayableBehaviour
    {
        private bool isInitialized;
        protected bool preventInitialization = false;


        protected virtual void Start() {}

        protected virtual void Stop() {}


        #region Unity events
        public override void OnBehaviourPause(Playable playable, FrameData info) => RunStop();

        public override void OnPlayableDestroy(Playable playable) => RunStop();

        public override void OnGraphStop(Playable playable) => RunStop();
        #endregion


        protected void RunStart()
        {
            if (isInitialized || preventInitialization)
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


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            RunStart();
        }
    }
}
