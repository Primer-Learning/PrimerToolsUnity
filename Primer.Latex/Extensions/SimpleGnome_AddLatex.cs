using UnityEngine;
using UnityEngine.UI;

namespace Primer.Latex
{
    public static class SimpleGnome_AddLatexExtensions
    {
        public static LatexComponent AddLatex(this SimpleGnome gnome, string formula, string name = null)
        {
            var child = gnome.Add<LatexComponent>("LaTex", name ?? formula);

            child.Process(formula);
            child.SetActive(true);

            return child;
        }
    }
}
