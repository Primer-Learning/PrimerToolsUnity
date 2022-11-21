using System;
using Primer.Timeline;
using UnityEngine;

namespace CommonTimelineAdapters
{
    [Serializable]
    public class ScaleSmoothly : ScrubbableAdapter
    {
        public enum Direction
        {
            ScaleUp,
            ScaleDown
        }

        public ExposedReference<Transform> _target;
        Transform target => Resolve(_target);

        public Direction direction;
        Vector3 originalScale;

        public override void Update(UpdateArgs args) {
            Vector3 from, to;

            if (direction == Direction.ScaleDown) {
                from = originalScale;
                to = Vector3.zero;
            }
            else {
                from = Vector3.zero;
                to = originalScale;
            }

            target.localScale = Vector3.Lerp(from, to, (float)args.time);
        }

        public override void RegisterPreviewingProperties(PropertyRegistrar registrar) {
            registrar.AddProperties(
                target,
                "m_LocalScale.x",
                "m_LocalScale.y",
                "m_LocalScale.z"
            );
        }

        public override void Prepare() {
            originalScale = target.localScale;
        }

        public override void Cleanup() {
            target.localScale = originalScale;
        }
    }
}
