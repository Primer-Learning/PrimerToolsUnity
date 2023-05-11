using UnityEngine;

namespace Primer.Shapes
{
    public static class ContainerExtensions
    {
        private static PrimerArrow2 arrowPrefabCache;
        private static PrimerArrow2 arrowPrefab => arrowPrefabCache ??= Resources.Load<PrimerArrow2>(PrimerArrow2.PREFAB_NAME);

        public static PrimerArrow2 AddArrow<T>(this Container<T> children, string name = null) where T : Component
        {
            return children.Add(arrowPrefab, name);
        }
    }
}
