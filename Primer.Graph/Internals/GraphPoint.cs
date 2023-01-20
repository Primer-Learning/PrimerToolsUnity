using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public class GraphPoint : MonoBehaviour
    {
        [Required]
        public PrefabProvider<Transform> prefab;

        private Graph graphCache;
        private Graph graph => graphCache ??= GetComponentInParent<Graph>();

        public Vector3 GetPositionMultiplier()
            => graph.positionMultiplier;

        public Vector3 GetScaleNeutralizer()
            => graph.GetScaleNeutralizer(prefab.scale);
    }
}
