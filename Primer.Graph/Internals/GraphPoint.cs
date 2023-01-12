using UnityEngine;

namespace Primer.Graph
{
    public class GraphPoint : MonoBehaviour
    {
        public Transform prefab;

        public Vector3 GetScaleNeutralizer()
        {
            var domainScale = transform.parent.localScale;
            var prefabScale = prefab.localScale;

            return new Vector3(
                prefabScale.x / domainScale.x,
                prefabScale.y / domainScale.y,
                prefabScale.z / domainScale.z
            );
        }
    }
}
