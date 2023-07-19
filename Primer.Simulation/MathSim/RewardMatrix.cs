using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{
    [Serializable]
    public class RewardMatrix<T> : IEnumerable<(T, T, float)> where T : Enum
    {
        [Serializable]
        private struct Entry
        {
            public T a;
            public T b;
            public float value;
        }

        [SerializeField] private List<Entry> entries = new();

        // private readonly Dictionary<T, Dictionary<T, float>> data = new();

        public float this[T a, T b] => Get(a, b);

        public RewardMatrix(float[,] values)
        {
            var types = EnumUtil.Values<T>();
            var cols = values.GetLength(0);
            var rows = values.GetLength(1);

            if (cols != types.Length)
                throw new ArgumentException("First dimension of values must match number of enum values");

            if (rows != types.Length)
                throw new ArgumentException("Second dimension of values must match number of enum values");

            for (var i = 0; i < cols; i++)
            for (var j = 0; j < rows; j++)
                Set(types[i], types[j], values[i, j]);
        }

        public float Get(T a, T b)
        {
            var index = entries.FindIndex(x => x.a.Equals(a) && x.b.Equals(b));

            if (index == -1)
                throw new ArgumentException($"No entry found for {a} and {b}");

            return entries[index].value;
        }

        public void Set(T a, T b, float value)
        {
            var index = entries.FindIndex(x => x.a.Equals(a) && x.b.Equals(b));

            if (index == -1)
                entries.Add(new Entry { a = a, b = b, value = value });
            else
                entries[index] = new Entry { a = a, b = b, value = value };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<(T, T, float)> GetEnumerator()
        {
            var types = EnumUtil.Values<T>();

            foreach (var a in types)
            foreach (var b in types)
                yield return (a, b, this[a, b]);
        }
    }
}
