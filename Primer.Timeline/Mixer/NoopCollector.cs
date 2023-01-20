using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class NoopCollector : IMixerCollector<bool>
    {
        private static readonly IEnumerable<(float, bool)> empty = Array.Empty<(float, bool)>();
        private static readonly IReadOnlyList<float> emptyWeights = Array.Empty<float>();
        private static readonly IReadOnlyList<bool> emptyInputs = Array.Empty<bool>();


        public int count => 0;
        public bool isEmpty => true;
        public bool isFull => true;

        public (float, bool) this[int i] => throw new ArgumentOutOfRangeException(nameof(i));


        public IEnumerator<(float, bool)> GetEnumerator() => empty.GetEnumerator();

        public IReadOnlyList<float> weights => emptyWeights;
        public IReadOnlyList<bool> inputs => emptyInputs;


        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Collect(Playable playable) {}

        public void AddInitialState(bool initial) {}

        public void Clear() {}
    }
}
