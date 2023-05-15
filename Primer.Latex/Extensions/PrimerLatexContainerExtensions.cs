using UnityEngine;

namespace Primer.Latex
{
    public static class PrimerLatexContainerExtensions
    {
        public static LatexComponent AddLatex<T>(this Container<T> container, string formula, string name = null,
            bool worldPositionStays = false) where T : Component
        {
            var child = container.AddPrefab<T, LatexComponent>(
                LatexComponent.PREFAB_NAME,
                name ?? LatexComponent.PREFAB_NAME,
                worldPositionStays
            );

            child.Process(formula);
            child.transform.SetScale(0);
            child.SetActive(true);

            return child;
        }
    }
}
