using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerPlayable : PlayableBehaviour
    {
        private Lifecycle _lifecycle;
        protected Lifecycle lifecycle => _lifecycle ??= new Lifecycle(Start, Stop);


        protected virtual void Start() {}

        protected virtual void Stop() {}


        public override void OnBehaviourPause(Playable playable, FrameData info) => lifecycle.Reset();
        public override void OnPlayableDestroy(Playable playable) => lifecycle.Reset();
        public override void OnGraphStop(Playable playable) => lifecycle.Reset();


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            lifecycle.Initialize();
        }
    }

}
