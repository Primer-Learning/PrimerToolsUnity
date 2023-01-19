using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public class GraphPoint : MonoBehaviour
    {
        [Required]
        public PrefabProvider<Transform> prefab;

        private Graph2 graphCache;
        private Graph2 graph => graphCache ??= GetComponentInParent<Graph2>();

        public Vector3 GetPositionMultiplier()
            => graph.positionMultiplier;

        public Vector3 GetScaleNeutralizer()
            => graph.GetScaleNeutralizer(prefab.scale);
    }
}
