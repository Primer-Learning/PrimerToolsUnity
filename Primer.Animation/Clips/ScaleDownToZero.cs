using System.Collections.Generic;
using UnityEngine;

namespace Primer.Animation
{
    public record ScaleDownToZero(Vector3 scale, double timeDouble, float duration = 0.5f) : IPrimerAnimation
    {
        public string name => "Scale down to zero";

        public void ApplyTo(AnimationClip clip)
        {
            var time = (float)timeDouble;
            var endTime = time + duration;

            clip.AddCurves<Transform>(new Dictionary<string, AnimationCurve> {
                ["m_LocalScale.x"] = IPrimerAnimation.CubicCurve(time, scale.x, endTime, 0),
                ["m_LocalScale.y"] = IPrimerAnimation.CubicCurve(time, scale.y, endTime, 0),
                ["m_LocalScale.z"] = IPrimerAnimation.CubicCurve(time, scale.z, endTime, 0),
            });
        }
    }
}
