using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    [Serializable]
    public class ScaleSmoothly : Scrubbable<Transform>, IPropertyModifier
    {
        public enum Direction { ScaleUp, ScaleDown }
        public Direction direction = Direction.ScaleUp;
        private Vector3 originalScale;


        public override void Prepare() => originalScale = target.localScale;
        public override void Cleanup() => target.localScale = originalScale;


        public override void Update(float t)
        {
            Vector3 from, to;

            if (direction == Direction.ScaleDown) {
                from = originalScale;
                to = Vector3.zero;
            }
            else {
                from = Vector3.zero;
                to = originalScale;
            }

            target.localScale = Vector3.Lerp(from, to, t);
        }


        public void RegisterProperties(IPropertyCollector registrar)
        {
            registrar.AddProperties(
                target,
                "m_LocalScale.x",
                "m_LocalScale.y",
                "m_LocalScale.z"
            );
        }
    }
}
