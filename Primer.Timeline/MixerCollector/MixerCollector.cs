using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class MixerCollector<T> : MixerCollector<T, T>
        where T : class, IPlayableBehaviour, new()
    {
        public MixerCollector() : base(x => x) {}
    }

    public class MixerCollector<TPlayableBehaviour, TData> : IMixerCollector<TData>
        where TPlayableBehaviour : class, IPlayableBehaviour, new()
    {
        private readonly Func<TPlayableBehaviour, TData> getData;
        protected float weight;

        private readonly List<float> _weights = new();
        public IReadOnlyList<float> weights => _weights;
        private readonly List<TData> _inputs = new();
        public IReadOnlyList<TData> inputs => _inputs;

        public MixerCollector(Func<TPlayableBehaviour, TData> getData) => this.getData = getData;

        public bool isEmpty => weight <= 0;
        public bool isFull => weight >= 1;
        public int count => inputs.Count;

        public (float weight, TData input) this[int i] => (weights[i], inputs[i]);

        public void Clear()
        {
            weight = 0;
            _weights.Clear();
            _inputs.Clear();
        }

        public virtual void Collect(Playable playable)
        {
            var inputCount = playable.GetInputCount();

            for (var i = 0; i < inputCount; i++) {
                var inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<TPlayableBehaviour>)playable.GetInput(i);

                if (inputWeight == 0 || inputPlayable.GetBehaviour() is not {} behaviour)
                    continue;

                weight += inputWeight;
                _weights.Add(inputWeight);
                _inputs.Add(getData(behaviour));
            }
        }

        public void AddInitialState(TData initial)
        {
            if (isFull)
                return;

            _weights.Insert(0, 1 - weight);
            _inputs.Insert(0, initial);
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(float, TData)> GetEnumerator()
            => inputs.Select((input, i) => (weights[i], input)).GetEnumerator();
    }
}
