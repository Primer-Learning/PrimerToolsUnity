using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public record ScaleUpFromZero(Vector3 scale, double doubleTime, float duration = 0.5f) : IPrimerAnimation
    {
        public string name => "Scale up from zero";

        public void ApplyTo(AnimationClip clip)
        {
            var time = (float)doubleTime;
            var endTime = time + duration;

            clip.AddCurves<Transform>(new Dictionary<string, AnimationCurve> {
                { "m_LocalScale.x", IPrimerAnimation.CubicCurve(time, 0, endTime, scale.x) },
                { "m_LocalScale.y", IPrimerAnimation.CubicCurve(time, 0, endTime, scale.y) },
                { "m_LocalScale.z", IPrimerAnimation.CubicCurve(time, 0, endTime, scale.z) },
            });
        }
    }
}
