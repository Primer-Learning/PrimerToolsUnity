using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Obsolete("Use PrimerMixer instead")]
    public abstract class PrimerBoundPlayable<T> : PrimerPlayable
    {
        protected bool isInitialized;
        private T trackTarget;

        protected abstract void Start(T trackTarget);

        protected abstract void Stop(T trackTarget);

        protected abstract void Frame(T trackTarget, Playable playable, FrameData info);


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (playerData is null) {
                return;
            }

            if (playerData is not T boundObject) {
                throw new Exception(
                    $"Expected track target to be {typeof(T).Name} but {playerData?.GetType().Name} found"
                );
            }

            trackTarget = boundObject;
            Frame(boundObject, playable, info);
        }


        public override void OnBehaviourPause(Playable playable, FrameData info) => RunStop();

        public override void OnPlayableDestroy(Playable playable) => RunStop();

        public override void OnGraphStop(Playable playable) => RunStop();


        protected void RunStart(T trackTarget)
        {
            if (isInitialized)
                return;

            Start(trackTarget);
            this.trackTarget = trackTarget;
            isInitialized = true;
        }

        protected void RunStop()
        {
            if (!isInitialized)
                return;

            Stop(trackTarget);
            isInitialized = false;
            trackTarget = default(T);
        }


        protected (List<float> weights, List<T> states) CollectInputs<T>(Playable playable)
            where T : class, IPlayableBehaviour, new()
        {
            var count = playable.GetInputCount();
            var weights = new List<float>();
            var states = new List<T>();

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<T>)playable.GetInput(i);

                if (weight == 0 || inputPlayable.GetBehaviour() is not T behaviour)
                    continue;

                weights.Add(weight);
                states.Add(behaviour);
            }

            return (weights, states);
        }
    }
}
