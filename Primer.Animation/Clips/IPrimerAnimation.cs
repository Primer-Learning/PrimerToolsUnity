using UnityEngine;

namespace Primer.Animation
{
    public interface IPrimerAnimation
    {
        string name { get; }
        void ApplyTo(AnimationClip clip);


        public static AnimationCurve cubic = CubicCurve(0, 0, 1, 1);

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

        public static AnimationCurve CubicCurve(AnimationCurve clip)
            => new(CubicCurve(clip.keys));

        public static Keyframe[] CubicCurve(Keyframe[] keys)
        {
            var result = new Keyframe[keys.Length];
            var length = keys.Length;

            for (var i = 0; i < length; i++) {
                result[i] = new Keyframe(keys[i].time, keys[i].value, 0.0f, 0.0f) {
                    inWeight = 2f / 3f,
                    outWeight = 2f / 3f,
                    weightedMode =
                        i == 0 ? WeightedMode.Out :
                        i == length - 1 ? WeightedMode.In :
                        WeightedMode.Both,
                };
            }

            return result;
        }
    }
}
