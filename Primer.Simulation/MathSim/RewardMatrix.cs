using System;
using System.Collections;
using System.Collections.Generic;

namespace Primer.Simulation
{
    public class RewardMatrix<T> : IEnumerable<(T, T, float)> where T : Enum
    {
        private readonly Dictionary<T, Dictionary<T, float>> data = new();

        public float this[T a, T b] {
            get => data[a][b];
            init {
                if (!data.ContainsKey(a))
                    data[a] = new Dictionary<T, float>();

                data[a][b] = value;
            }
        }

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
                this[types[i], types[j]] = values[i, j];
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
