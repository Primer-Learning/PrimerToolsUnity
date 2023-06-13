using UnityEngine;

namespace Primer.Latex
{
    public static class Container_AddLatexExtensions
    {
        public static LatexComponent AddLatex<T>(this Container<T> container, string formula, string name = null,
            bool worldPositionStays = false) where T : Component
        {
            var child = container.AddPrefab<T, LatexComponent>(
                LatexComponent.PREFAB_NAME,
                name ?? formula,
                worldPositionStays
            );

            child.Process(formula);
            child.transform.SetScale(0);
            child.SetActive(true);

            return child;
        }
    }
}
