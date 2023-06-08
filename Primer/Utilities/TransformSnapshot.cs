using UnityEngine;

namespace Primer
{
    public class TransformSnapshot
    {
        private readonly Transform parent;

        public Vector3 position { get; }
        public Quaternion rotation { get; }
        public Vector3 scale { get; }


        public TransformSnapshot(Transform target)
        {
            parent = target.parent;
            position = target.localPosition;
            rotation = target.localRotation;
            scale = target.localScale;
        }


        public void ApplyTo(Transform other, Vector3? offsetPosition = null)
        {
            if (other.parent != parent)
                other.SetParent(parent, worldPositionStays: false);

            other.localPosition = offsetPosition is null
                ? position
                : position + rotation * Vector3.Scale(offsetPosition.Value, scale);

            other.localRotation = rotation;
            other.localScale = scale;
        }

        public override string ToString()
        {
            return $"pos({position}) rot({rotation}) scale({scale})";
        }
    }
}
