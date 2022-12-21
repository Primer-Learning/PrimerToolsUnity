using System.Collections.Generic;
using Unity.VectorGraphics;

namespace Primer.Latex
{
    public static class LatexSymbolStorage
    {
        private static readonly Dictionary<int, LatexSymbol> storage = new();

        public static LatexSymbol FromGeometry(VectorUtils.Geometry geometry)
        {
            var hash = geometry.CalculateHashCode();

            if (storage.ContainsKey(hash)) {
                return storage[hash];
            }

            var newSymbol = new LatexSymbol(geometry);
            storage.Add(hash, newSymbol);
            return newSymbol;
        }
    }
}
