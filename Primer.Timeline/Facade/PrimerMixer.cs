using System;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public abstract class PrimerMixer<T> : PrimerPlayable
    {
        protected abstract void Start(T trackTarget);
        protected abstract void Stop(T trackTarget);
        protected abstract void Frame(T trackTarget, Playable playable, FrameData info);


        protected bool isInitialized;
        T trackTarget;


        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            base.ProcessFrame(playable, info, playerData);

            if (playerData is null) {
                return;
            }

            if (playerData is not T trackTarget) {
                throw new Exception($"Expected track target to be {typeof(T).Name} but {playerData?.GetType().Name} found");
            }

            this.trackTarget = trackTarget;

            if (!isInitialized) {
                RunStart(trackTarget);
            }

            Frame(trackTarget, playable, info);
        }


        public override void OnBehaviourPause(Playable playable, FrameData info) => RunStop();
        public override void OnPlayableDestroy(Playable playable) => RunStop();
        public override void OnGraphStop(Playable playable) => RunStop();


        protected void RunStart(T trackTarget) {
            if (isInitialized) return;
            Start(trackTarget);
            this.trackTarget = trackTarget;
            isInitialized = true;
        }

        protected void RunStop() {
            if (!isInitialized) return;
            Stop(trackTarget);
            isInitialized = false;
            trackTarget = default;
        }
    }
}
