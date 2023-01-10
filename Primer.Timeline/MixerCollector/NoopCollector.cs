using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class NoopCollector : IMixerCollector<bool>
    {
        private static readonly (float, bool)[] empty = Array.Empty<(float, bool)>();

        public int count => 0;
        public bool isEmpty => true;
        public bool isFull => true;

        public (float, bool) this[int i] => throw new ArgumentOutOfRangeException(nameof(i));

        public IEnumerator<(float, bool)> GetEnumerator() => ((IEnumerable<(float, bool)>)empty).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Collect(Playable playable) {}

        public void AddInitialState(bool initial) {}

        public void Clear() {}
    }
}
