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

    public abstract class PrimerPlayable<T> : PrimerPlayable where T : PrimerBehaviour
    {
        protected bool isInitialized;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            if (playerData is not T trackTarget) {
                throw new Exception($"Expected track target to be {typeof(T).Name} but {playerData.GetType().Name} found");
            }

            if (!isInitialized) {
                isInitialized = true;
                Start(trackTarget);
            }

            Frame(trackTarget, playable, info);
        }

        public abstract void Frame(T trackTarget, Playable playable, FrameData info);

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            base.OnBehaviourPause(playable, info);
            isInitialized = false;
            Stop();
        }

        public abstract void Start(T trackTarget);
        public abstract void Stop();

    }
}
