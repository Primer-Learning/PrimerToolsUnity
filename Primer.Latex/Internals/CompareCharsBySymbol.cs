using System.Collections.Generic;

namespace Primer.Latex
{
    internal class CompareCharsBySymbol : IEqualityComparer<LatexChar>
    {
        public bool Equals(LatexChar x, LatexChar y)
        {
            if (x == y) return true;
            if (x is null || y is null) return false;
            return x.IsSameSymbol(y);
        }

        public int GetHashCode(LatexChar obj)
        {
            return obj.symbol.GetHashCode();
        }
    }
}
