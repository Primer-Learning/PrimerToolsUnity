using UnityEngine;

namespace Primer.Latex
{
    public static class Container_AddLatexExtensions
    {
        public static LatexComponent AddLatex<T>(this Container<T> container, string formula, string name = null) where T : Component
        {
            var child = container.AddPrefab<T, LatexComponent>(LatexComponent.PREFAB_NAME, name ?? formula);

            child.Process(formula);
            child.transform.SetScale(0);
            child.SetActive(true);

            return child;
        }
    }
}
