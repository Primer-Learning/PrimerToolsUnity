using System.Collections.Generic;
using UnityEngine;

namespace LatexRenderer
{
    public static class SuperBounds
    {
        /// <summary>Calculates the smallest Bounds object that contains all given Bounds objects.</summary>
        public static Bounds GetSuperBounds(IEnumerable<Bounds> allBounds)
        {
            var min = Vector3.positiveInfinity;
            var max = Vector3.negativeInfinity;
            foreach (var bounds in allBounds)
            {
                min = Vector3.Min(min, bounds.min);
                max = Vector3.Max(max, bounds.max);
            }

            var result = new Bounds();
            result.SetMinMax(min, max);
            return result;
        }
    }
}