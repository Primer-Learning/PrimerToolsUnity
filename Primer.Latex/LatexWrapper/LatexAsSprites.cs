using System;
using System.Linq;
using UnityEngine;

namespace Primer.Latex
{
    public sealed record LatexAsSprites(Sprite[] Sprites, Vector3[] Positions)
    {
        public bool Equals(LatexAsSprites other) {
            return other != null &&
                   // We compare them with == because we want to know if both of them are null
                   // or both o them are the same object, in both cases we don't compare their content
                   // ReSharper disable once PossibleUnintendedReferenceComparison
                   (Sprites == other.Sprites || Sprites.SequenceEqual(other.Sprites)) &&
                   // ReSharper disable once PossibleUnintendedReferenceComparison
                   (Positions == other.Positions || Positions.SequenceEqual(other.Positions));
        }

        public override int GetHashCode() => HashCode.Combine(Sprites, Positions);
    }
}
