using UnityEngine;

namespace Primer
{
    public class TransformSnapshot
    {
        private readonly Transform target;
        private readonly Transform parent;

        public readonly Vector3 position;
        public readonly Quaternion rotation;
        public readonly Vector3 localScale;


        public TransformSnapshot(Transform target)
        {
            this.target = target;
            parent = target.parent;
            position = target.position;
            rotation = target.rotation;
            localScale = target.localScale;
        }


        public void Restore() => ApplyTo(target);

        public void ApplyTo(Transform other)
        {
            if (other.parent != parent)
                other.parent = parent;

            other.position = position;
            other.rotation = rotation;
            other.localScale = localScale;
        }


        public override string ToString()
            => $"pos({position}) rot({rotation}) scale({localScale})";
    }
}