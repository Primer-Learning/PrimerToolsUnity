using UnityEngine;

namespace Primer.Shapes
{
    public static class PrimerShapesContainerExtensions
    {
        public static PrimerArrow2 AddArrow<T>(this Container<T> children, string name = null,
            bool worldPositionStays = false) where T : Component
        {
            return children.AddPrefab<T, PrimerArrow2>(PrimerArrow2.PREFAB_NAME, name, worldPositionStays);
        }

        public static PrimerBracket2 AddBracket<T>(this Container<T> children, string name = null,
            bool worldPositionStays = false) where T : Component
        {
            return children.AddPrefab<T, PrimerBracket2>(PrimerBracket2.PREFAB_NAME, name, worldPositionStays);
        }
    }
}
