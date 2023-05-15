using UnityEngine;

namespace Primer.Latex
{
    public static class ContainerExtensions
    {
        private static LatexComponent prefabCache;

        private static LatexComponent prefab
            => prefabCache ??= Resources.Load<LatexComponent>(LatexComponent.PREFAB_NAME);

        public static LatexComponent AddLatex<T>(this Container<T> container, string formula, string name = null,
            bool worldPositionStays = false) where T : Component
        {
            var child = container.Add(prefab, name ?? LatexComponent.PREFAB_NAME, worldPositionStays);
            child.Process(formula);
            child.transform.SetScale(0);
            return child;
        }
    }
}
