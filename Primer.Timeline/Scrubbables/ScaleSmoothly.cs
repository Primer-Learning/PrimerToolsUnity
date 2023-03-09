using System;
using Primer.Timeline;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Timeline;

namespace Primer.Animation
{
    [Serializable]
    public class ScaleSmoothly : Scrubbable, IPropertyModifier
    {
        [EnumToggleButtons]
        public enum Direction { ScaleUp, ScaleDown }

        private Vector3 originalScale;

        [Space] public Direction direction = Direction.ScaleUp;
        [Space] public EaseMode ease = EaseMode.Cubic;


        public override void Prepare()
        {
            originalScale = target.localScale;
        }

        public override void Cleanup()
        {
            target.localScale = originalScale;
        }


        public override void Update(float t)
        {
            // Execute(0) is now called instead of Cleanup. So calling this cleanup here for now.
            if (t == 0)
            {
                Cleanup();
            }
            Vector3 from, to;

            if (direction == Direction.ScaleDown) {
                from = originalScale;
                to = Vector3.zero;
            }
            else {
                from = Vector3.zero;
                to = originalScale;
            }

            target.localScale = Vector3.Lerp(from, to, ease.Apply(t));
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
