using System.Collections.Generic;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public abstract class CollectedMixer<T, U> : PrimerMixer<T>
    {
        protected U originalValue;
        float lastTotalWeight;


        protected abstract U ProcessPlayable(PrimerPlayable behaviour);
        protected abstract U SingleInput(U input, float weight, bool isReverse);
        protected abstract U Mix(List<float> weights, List<U> inputs);
        protected abstract void Apply(T trackTarget, U input);


        protected override void Frame(T trackTarget, Playable playable, FrameData info) {
            var count = playable.GetInputCount();

            var weights = new List<float>();
            var inputs = new List<U>();
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

            var isDecreasing = totalWeight < lastTotalWeight;
            lastTotalWeight = totalWeight;

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
                    var singleResult = SingleInput(inputs[0], weights[0], isDecreasing);
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
