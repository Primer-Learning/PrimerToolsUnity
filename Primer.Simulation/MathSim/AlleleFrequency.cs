using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer.Simulation
{
    [Serializable]
    public class AlleleFrequency<T> : IEnumerable<(T, float)> where T : Enum
    {
        private static readonly T[] keys = EnumUtil.Values<T>();
        [SerializeField] private float[] alleles = new float[keys.Length];

        public float this[T allele] {
            get => alleles[Array.IndexOf(keys, allele)];
            set => alleles[Array.IndexOf(keys, allele)] = value;
        }

        public AlleleFrequency() {}

        public AlleleFrequency(params float[] values)
        {
            if (values.Length != keys.Length)
                throw new ArgumentException("Number of values must match number of enum values");

            for (var i = 0; i < values.Length; i++)
                values[i] = values[i];
        }

        public void Normalize()
        {
            var sum = alleles.Sum();

            for (var i = 0; i < keys.Length; i++)
                alleles[i] /= sum;
        }

        public float DeltaMagnitude(AlleleFrequency<T> other)
        {
            var result = 0f;

            for (var i = 0; i < keys.Length; i++)
                result += Math.Abs(alleles[i] - other.alleles[i]);

            return result;
        }

        public override int GetHashCode()
        {
            var hash = 17;

            for (var i = 0; i < keys.Length; i++)
                hash = hash * 23 + alleles[i].GetHashCode();

            return hash;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<(T, float)> GetEnumerator()
        {
            for (var i = 0; i < keys.Length; i++)
                yield return (keys[i], alleles[i]);
        }
    }
}
