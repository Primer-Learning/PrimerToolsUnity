using UnityEngine;

namespace Primer.Animation
{
    public interface IPrimerAnimation
    {
        string name { get; }
        void ApplyTo(AnimationClip clip);

        public static AnimationCurve CubicCurve(float startTime, float startValue, float endTime, float endValue)
        {
            if (startTime == endTime) {
                return new AnimationCurve(new Keyframe(startTime, startValue));
            }

            var start = new Keyframe(startTime, startValue, 0.0f, 0.0f);
            var end = new Keyframe(endTime, endValue, 0.0f, 0.0f);

            start.weightedMode = WeightedMode.Out;
            end.weightedMode = WeightedMode.In;

            start.outWeight = 2f / 3f;
            end.inWeight = 2f / 3f;

            return new AnimationCurve(start, end);
        }
    }
}
