using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Primer.Simulation.Strategy;
using UnityEngine;

namespace Primer.Simulation
{
    [Serializable]
    public class RewardMatrix : IEnumerable<(Type, Type, float)>
    {
        [Serializable]
        private struct Entry
        {
            public Type a;
            public Type b;
            public float value;
        }

        [SerializeField] private List<Entry> entries = new();

        private Type[] strategies; 

        public float this[Type a, Type b] => Get(a, b);

        public RewardMatrix(Type[] strategies, float[,] values)
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

        public float Get(Type a, Type b)
        {
            var index = entries.FindIndex(x => x.a.Equals(a) && x.b.Equals(b));

            if (index == -1)
                throw new ArgumentException($"No entry found for {a} and {b}");

            return entries[index].value;
        }

        public void Set(Type a, Type b, float value)
        {
            var index = entries.FindIndex(x => x.a.Equals(a) && x.b.Equals(b));

            if (index == -1)
                entries.Add(new Entry { a = a, b = b, value = value });
            else
                entries[index] = new Entry { a = a, b = b, value = value };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(Type, Type, float)> GetEnumerator()
        {
            foreach (var a in strategies)
            foreach (var b in strategies)
                yield return (a, b, this[a, b]);
        }
    }
}
