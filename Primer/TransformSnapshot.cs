using UnityEngine;

namespace Primer
{
    [ExecuteAlways]
    public class TransformSnapshot : MonoBehaviour
    {
        public Vector3 localScale;
        private Transform parent;
        public Vector3 position;
        public Quaternion rotation;


        private void Awake() => hideFlags = HideFlags.DontSave;

        private void OnEnable() => Snapshot();

        private void OnDisable() => Restore();


        public void Snapshot()
        {
            var me = transform;
            parent = me.parent;
            position = me.position;
            rotation = me.rotation;
            localScale = me.localScale;
        }

        public void Restore()
        {
            ApplyTo(transform);
        }

        public void ApplyTo(Transform other)
        {
            if (other.parent != parent)
                other.parent = parent;

            other.position = position;
            other.rotation = rotation;
            other.localScale = localScale;
        }
    }
}
