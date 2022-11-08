using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerPlayable : PlayableBehaviour
    {
        [HideInInspector]
        public double duration;
    }

    public abstract class PrimerPlayable<T> : PrimerPlayable
    {
        public abstract void Start(T trackTarget);
        public abstract void Stop(T trackTarget);
        public abstract void Frame(T trackTarget, Playable playable, FrameData info);

        protected bool isInitialized;
        T trackTarget;
        double frameTime;
        public float time => (float)(frameTime / duration);

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            base.ProcessFrame(playable, info, playerData);

            if (playerData is null) {
                return;
            }

            if (playerData is not T trackTarget) {
                throw new Exception($"Expected track target to be {typeof(T).Name} but {playerData?.GetType().Name} found");
            }

            if (!isInitialized) {
                isInitialized = true;
                this.trackTarget = trackTarget;
                Start(trackTarget);
            }

            frameTime = playable.GetTime();
            Frame(trackTarget, playable, info);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            base.OnBehaviourPause(playable, info);
            isInitialized = false;
            Stop(trackTarget);
            trackTarget = default;
        }
    }
}
