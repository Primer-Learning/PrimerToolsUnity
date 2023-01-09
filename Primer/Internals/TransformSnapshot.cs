using UnityEngine;

namespace Primer
{
    public struct TransformSnapshot
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;

        public TransformSnapshot(Transform transform)
        {
            position = transform.position;
            rotation = transform.rotation;
            localScale = transform.localScale;
        }

        public void CopyTo(Transform transform)
        {
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = localScale;
        }
    }
}
