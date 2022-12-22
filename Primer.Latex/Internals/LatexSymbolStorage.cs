using System.Collections.Generic;
using Unity.VectorGraphics;

namespace Primer.Latex
{
    public static class LatexSymbolStorage
    {
        private static readonly List<LatexSymbol> storage = new();

        public static LatexSymbol FromGeometry(VectorUtils.Geometry geometry)
        {
            var existing = storage.Find(x => x.geometry.IsSimilarEnough(geometry));

            if (existing is not null) {
                return existing;
            }

            var newSymbol = new LatexSymbol(geometry);
            storage.Add(newSymbol);
            return newSymbol;
        }
    }
}
