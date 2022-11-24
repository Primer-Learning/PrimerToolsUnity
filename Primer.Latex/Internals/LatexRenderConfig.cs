using System;
using System.Collections.Generic;
using System.Linq;

namespace Primer.Latex
{
    public sealed record LatexRenderConfig(string Latex, IReadOnlyList<string> Headers)
    {
        public bool Equals(LatexRenderConfig other) {
            return other != null &&
                Latex == other.Latex &&
                // We compare them with == because we want to know if both of them are null
                // or both o them are the same object, in both cases we don't compare their content
                // ReSharper disable once PossibleUnintendedReferenceComparison
                (Headers == other.Headers || Headers.SequenceEqual(other.Headers));
        }

        public override int GetHashCode() => HashCode.Combine(Latex, Headers);
    }
}
