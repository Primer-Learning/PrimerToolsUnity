using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Evolution.Extensions
{
    public static class EnumerableExtensions
    {
        public static T RandomItem<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable as List<T> ?? enumerable.ToList();
            var index = Random.Range(0, list.Count);
            return list[index];
        }
    }
}
