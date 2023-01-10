using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Obsolete("Use PrimerMixer instead")]
    public abstract class CollectedMixer<TTrackTarget, TData> : PrimerBoundPlayable<TTrackTarget>
    {
        protected TData originalValue;
        float lastTotalWeight;
        double lastTime;

        protected abstract TData ProcessPlayable(PrimerPlayable behaviour);
        protected abstract TData SingleInput(TData input, float weight, bool isReverse);
        protected abstract TData Mix(List<float> weights, List<TData> inputs);
        protected abstract void Apply(TTrackTarget trackTarget, TData input);


        protected override void Frame(TTrackTarget trackTarget, Playable playable, FrameData info) {
            var count = playable.GetInputCount();

            var weights = new List<float>();
            var inputs = new List<TData>();
            var totalWeight = 0f;

            for (var i = 0; i < count; i++) {
                var weight = playable.GetInputWeight(i);
                if (weight == 0) continue;

                var inputPlayable = (ScriptPlayable<PrimerPlayable>)playable.GetInput(i);
                var behaviour = inputPlayable.GetBehaviour();
                if (behaviour is null) continue;

                var value = ProcessPlayable(behaviour);
                if (value is null) continue;

                weights.Add(weight);
                inputs.Add(value);
                totalWeight += weight;
            }

            var isDecreasing = lastTotalWeight > totalWeight;
            var isBackwards = lastTime > playable.GetTime();
            lastTotalWeight = totalWeight;
            lastTime = playable.GetTime();

            // No weights, we stop mixing
            if (totalWeight == 0) {
                RunStop();
                return;
            }

            RunStart(trackTarget);

            // Weight lower that 1 means
            // the only playable is appearing / disappearing
            if (totalWeight < 1) {
                // If we have original value we add it to the input list
                // otherwise we invoke SingleInput() instead of Frame()
                if (originalValue is null) {
                    var isReverse = isBackwards ? !isDecreasing : isDecreasing;
                    var singleResult = SingleInput(inputs[0], weights[0], isReverse);
                    Apply(trackTarget, singleResult);
                    return;
                }

                weights.Add(1 - totalWeight);
                inputs.Add(originalValue);
            }

            var result = inputs.Count == 1 ? inputs[0] : Mix(weights, inputs);
            Apply(trackTarget, result);
        }
    }
}
