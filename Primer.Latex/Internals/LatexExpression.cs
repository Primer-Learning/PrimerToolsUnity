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
        [SerializeField]
        private LatexChar[] characters;
        private readonly LatexInput input;

        public Vector2 center => GetBounds().center;
        public int count => characters.Length;
        public string source => input.code;


        public LatexExpression(LatexInput input)
        {
            this.input = input;
            characters = Array.Empty<LatexChar>();
        }

        public LatexExpression(LatexInput input, LatexChar[] chars)
        {
            this.input = input;
            characters = chars;
        }


        public bool IsSame(LatexExpression other)
        {
            return !ReferenceEquals(null, other) && characters.SequenceEqual(other.characters);
        }

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
            var modifiedInput = input with { code = "{input.code} |slice({start},{end})" };
            var chars = characters.Take(end).Skip(start).ToArray();
            return new LatexExpression(modifiedInput, chars);
        }

        public override string ToString()
        {
            return $"LatexExpression({input.code})[{characters.Length}]";
        }


        public IEnumerator<LatexChar> GetEnumerator() => characters.ToList().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
