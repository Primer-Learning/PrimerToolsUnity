using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VectorGraphics;
using UnityEngine;

namespace Primer.Latex
{
    [Serializable]
    public class LatexExpression : IEnumerable<LatexChar>
    {
        [SerializeReference]
        private readonly LatexChar[] characters;

        public bool isEmpty => characters.Length == 0;
        public Vector2 center => GetBounds().center;


        // public int count => characters.Length;
        // public LatexChar this[int index] => characters[index];


        public LatexExpression() => characters = Array.Empty<LatexChar>();

        public LatexExpression(LatexChar[] chars) => characters = chars;


        public bool IsSame(LatexExpression other)
            => !ReferenceEquals(null, other) && characters.SequenceEqual(other.characters);

        public Rect GetBounds()
        {
            var allVertices = new Vector2[characters.Length * 2];

            for (var i = 0; i < characters.Length; i++) {
                var charBounds = characters[i].bounds;
                allVertices[i * 2] = charBounds.min;
                allVertices[i * 2 + 1] = charBounds.max;
            }

            return VectorUtils.Bounds(allVertices);
        }

        public LatexExpression Slice(int start, int end)
        {
            var chars = characters.Skip(start).Take(end - start).ToArray();
            return new LatexExpression(chars);
        }


        public IEnumerator<LatexChar> GetEnumerator()
            => ((IEnumerable<LatexChar>)characters).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();


        #region Groups
        public IEnumerable<LatexExpression> Split(List<int> indexes) => Split(CalculateRanges(indexes));

        public IEnumerable<LatexExpression> Split(List<(int start, int end)> ranges)
            => ranges.Select(x => Slice(x.start, x.end));

        // TODO: Move to LatexGroups now
        public List<(int start, int end)> CalculateRanges(List<int> indexes)
        {
            var last = 0;
            var result = new List<(int, int)>();

            foreach (var start in indexes) {
                if (start == last)
                    continue;

                if (start >= characters.Length)
                    break;

                result.Add((last, start));
                last = start;
            }

            if (last != characters.Length)
                result.Add((last, characters.Length));

            return result;
        }
        #endregion
    }
}
