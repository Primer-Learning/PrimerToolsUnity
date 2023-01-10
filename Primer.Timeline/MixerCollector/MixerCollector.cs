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
        private readonly List<TData> data = new();
        private readonly Func<TPlayableBehaviour, TData> getData;
        private readonly List<float> weights = new();
        protected float weight;

        public MixerCollector(Func<TPlayableBehaviour, TData> getData) => this.getData = getData;

        public bool isEmpty => weight <= 0;
        public bool isFull => weight >= 1;
        public int count => data.Count;

        public (float, TData) this[int i] => (weights[i], data[i]);

        public void Clear()
        {
            weight = 0;
            weights.Clear();
            data.Clear();
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
                weights.Add(inputWeight);
                data.Add(getData(behaviour));
            }
        }

        public void AddInitialState(TData initial)
        {
            if (isFull)
                return;

            weights.Insert(0, 1 - weight);
            data.Insert(0, initial);
        }


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(float, TData)> GetEnumerator()
            => data.Select((input, i) => (weights[i], input)).GetEnumerator();
    }
}
