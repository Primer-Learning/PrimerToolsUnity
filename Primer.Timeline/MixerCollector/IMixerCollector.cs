using System.Collections.Generic;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public interface IMixerCollector<T> : IEnumerable<(float weight, T input)>
    {
        int count { get; }
        bool isEmpty { get; }
        bool isFull { get; }

        (float weight, T input) this[int i] { get; }

        void Collect(Playable playable);

        void AddInitialState(T initial);

        void Clear();
    }
}
