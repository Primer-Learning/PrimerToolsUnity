using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Primer.Simulation
{
    public class AlleleFrequency<T> : IEnumerable<KeyValuePair<T, float>> where T : Enum
    {
        public readonly Dictionary<T, float> alleles = new();

        public float this[T allele] {
            get => alleles[allele];
            set => alleles[allele] = value;
        }

        public AlleleFrequency() {}

        public AlleleFrequency(params float[] values)
        {
            var types = EnumUtil.Values<T>();

            if (values.Length != types.Length)
                throw new ArgumentException("Number of values must match number of enum values");

            for (var i = 0; i < values.Length; i++)
                alleles[types[i]] = values[i];
        }

        public void Normalize()
        {
            var sum = alleles.Values.Sum();
            var keys = alleles.Keys.ToList();

            foreach (var allele in keys)
                alleles[allele] /= sum;
        }

        public float DeltaMagnitude(AlleleFrequency<T> other)
        {
            return alleles.Keys.Sum(allele => Math.Abs(alleles[allele] - other[allele]));
        }

        public IEnumerator<KeyValuePair<T, float>> GetEnumerator() => alleles.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
