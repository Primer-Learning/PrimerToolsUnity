using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class ListExtensions
    {
        // Pulled from https://stackoverflow.com/a/1262619/3920202
        public static void Randomize<T>(this List<T> list)
        {
            var n = list.Count;

            while (n > 1) {
                n--;
                var k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }
    }
}
