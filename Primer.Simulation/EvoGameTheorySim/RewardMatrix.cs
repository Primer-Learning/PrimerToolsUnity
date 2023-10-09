using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Primer.Simulation
{
    [Serializable]
    public class RewardMatrix : IEnumerable<(SimultaneousTurnAction, SimultaneousTurnAction, float)>
    {
        [Serializable]
        private struct Entry
        {
            public SimultaneousTurnAction a;
            public SimultaneousTurnAction b;
            public float value;
        }

        [SerializeField] private List<Entry> entries = new();

        private SimultaneousTurnAction[] strategies; 

        public float this[SimultaneousTurnAction a, SimultaneousTurnAction b] => Get(a, b);

        public RewardMatrix(SimultaneousTurnAction[] strategies, float[,] values)
        {
            this.strategies = strategies;
            var cols = values.GetLength(0);
            var rows = values.GetLength(1);

            if (cols != strategies.Length)
                throw new ArgumentException("First dimension of values must match number of strategies");

            if (rows != strategies.Length)
                throw new ArgumentException("Second dimension of values must match number of strategies");

            for (var i = 0; i < cols; i++)
            for (var j = 0; j < rows; j++)
                Set(strategies[i], strategies[j], values[i, j]);
        }

        public float Get(SimultaneousTurnAction a, SimultaneousTurnAction b)
        {
            var index = entries.FindIndex(x => x.a.Equals(a) && x.b.Equals(b));

            if (index == -1)
                throw new ArgumentException($"No entry found for {a} and {b}");

            return entries[index].value;
        }

        public void Set(SimultaneousTurnAction a, SimultaneousTurnAction b, float value)
        {
            var index = entries.FindIndex(x => x.a.Equals(a) && x.b.Equals(b));

            if (index == -1)
                entries.Add(new Entry { a = a, b = b, value = value });
            else
                entries[index] = new Entry { a = a, b = b, value = value };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(SimultaneousTurnAction, SimultaneousTurnAction, float)> GetEnumerator()
        {
            foreach (var a in strategies)
            foreach (var b in strategies)
                yield return (a, b, this[a, b]);
        }
    }
}
