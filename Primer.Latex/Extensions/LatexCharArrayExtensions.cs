using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    public static class LatexCharArrayExtensions
    {
        public static Vector3 GetCenter(this LatexChar[] chars)
        {
            var allVertices = chars.SelectMany(@char => @char.symbol.geometry.TransformVertices());
            return VectorUtils.Bounds(allVertices).center;
        }

        public static List<(int start, int end)> GetRanges(this LatexChar[] chars, IEnumerable<int> groups)
        {
            var last = 0;
            var ranges = new List<(int, int)>();

            foreach (var start in groups) {
                if (start == last)
                    continue;

                if (start >= chars.Length)
                    break;

                ranges.Add((last, start));
                last = start;
            }

            ranges.Add((last, chars.Length));
            return ranges;
        }
    }
}
