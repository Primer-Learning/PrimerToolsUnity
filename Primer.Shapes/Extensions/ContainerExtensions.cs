using UnityEngine;

namespace Primer.Shapes
{
    public static class ContainerExtensions
    {
        public static PrimerArrow2 AddArrow(this Container children, string name = null)
        {
            return children.AddPrefab<PrimerArrow2>(PrimerArrow2.PREFAB_NAME, name);
        }
        
        public static PrimerArrow2 Add3DArrow(this Container children, string name = null)
        {
            return children.AddPrefab<PrimerArrow2>("3DArrow", name);
        }

        public static PrimerBracket2 AddBracket(this Container children, string name = null)
        {
            return children.AddPrefab<PrimerBracket2>(PrimerBracket2.PREFAB_NAME, name);
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
