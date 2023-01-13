using UnityEngine;

namespace Primer.Graph
{
    public class GraphPoint : MonoBehaviour
    {
        public Transform prefab;
        public Vector3 scalePrefab = Vector3.one;

        private Graph2 graphCache;
        private Graph2 graph => graphCache ??= GetComponentInParent<Graph2>();

        public Vector3 GetPositionMultiplier()
            => graph.positionMultiplier;

        public Vector3 GetScaleNeutralizer()
            => graph.GetScaleNeutralizer(Vector3.Scale(prefab.localScale, scalePrefab));
    }
}
