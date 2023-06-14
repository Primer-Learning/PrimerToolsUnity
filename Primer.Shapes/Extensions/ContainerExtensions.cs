using UnityEngine;

namespace Primer.Shapes
{
    public static class ContainerExtensions
    {
        public static PrimerArrow2 AddArrow<T>(this Container<T> children, string name = null) where T : Component
        {
            return children.AddPrefab<T, PrimerArrow2>(PrimerArrow2.PREFAB_NAME, name);
        }

        public static PrimerBracket2 AddBracket<T>(this Container<T> children, string name = null) where T : Component
        {
            return children.AddPrefab<T, PrimerBracket2>(PrimerBracket2.PREFAB_NAME, name);
        }

        public static Container<Follower> AddFollower<T>(this Container container, T follow, string name = null)
            where T : Component
        {
            var follower = container.AddContainer<Follower>(name ?? $"Follower({follow.name})");
            var transform = follow.transform;
            var component = follower.component;
            component.getter = () => component.useGlobalSpace ? transform.position : transform.localPosition;
            return follower;
        }
    }
}
