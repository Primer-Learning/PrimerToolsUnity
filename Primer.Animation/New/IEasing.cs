using UnityEngine;

namespace Primer.Animation
{
    public interface IEasing
    {
        public static IEasing defaultMethod = new CubicEasing();

        public float Evaluate(float t);
    }

    public class LinearEasing : Singleton<LinearEasing>, IEasing
    {
        public float Evaluate(float t) => t;
    }

    public class SmoothStepEasing : Singleton<SmoothStepEasing>, IEasing
    {
        public float Evaluate(float t) => Mathf.SmoothStep(0, 1, t);
    }

    public class DoubleSmoothStepEasing : Singleton<DoubleSmoothStepEasing>, IEasing
    {
        public float Evaluate(float t) => (Mathf.SmoothStep(0, 1, t) + t) / 2;
    }

    public class SmoothInEasing : Singleton<SmoothInEasing>, IEasing
    {
        public float Evaluate(float t) => Mathf.SmoothStep(0, 2, t / 2);
    }

    public class SmoothOutEasing : Singleton<SmoothOutEasing>, IEasing
    {
        public float Evaluate(float t) => Mathf.SmoothStep(0, 2, (t + 1) / 2) - 1;
    }

    public class CubicEasing : Singleton<CubicEasing>, IEasing
    {
        public float Evaluate(float t) => EaseInAndOutCubic(0, 1, t);

        private static float EaseInAndOutCubic(float startVal, float endVal, float t)
        {
            // Scale time relative to half duration
            t *= 2;

            switch (t) {
                // handle different
                case <= 0:
                    return startVal;

                case <= 1:
                    // Ease in from startVal to half the overall change
                    return (endVal - startVal) / 2 * t * t * t + startVal;

                case <= 2:
                    // Make t negative to use left side of cubic
                    t -= 2;

                    // Ease out from half to end of overall change
                    return (endVal - startVal) / 2 * t * t * t + endVal;

                default:
                    return endVal;
            }
        }
    }

    public class QuadraticEasing : Singleton<QuadraticEasing>, IEasing
    {
        public float Evaluate(float t) => EaseInAndOutQuadratic(0, 1, t);

        private static float EaseInAndOutQuadratic(float startVal, float endVal, float t)
        {
            // Scale time relative to half duration
            t *= 2;

            switch (t) {
                // handle different
                case <= 0:
                    return startVal;

                case <= 1:
                    // Ease in from startVal to half the overall change
                    return (endVal - startVal) / 2 * t * t + startVal;

                case <= 2:
                    // Make t negative to use left side of cubic
                    t -= 2;

                    // Ease out from half to end of overall change
                    return (endVal - startVal) / 2 * t * t + endVal;

                default:
                    return endVal;
            }
        }
    }

    public class CubicInEasing : Singleton<CubicInEasing>, IEasing
    {
        public float Evaluate(float t) => EaseInCubic(0, 1, t);

        private static float EaseInCubic(float startVal, float endVal, float t)
        {
            // Ease in from startVal
            return (endVal - startVal) * t * t * t + startVal;
        }
    }

    public class CubicOutEasing : Singleton<CubicOutEasing>, IEasing
    {
        public float Evaluate(float t) => EaseOutCubic(0, 1, t);

        private static float EaseOutCubic(float startVal, float endVal, float t)
        {
            // Ease out from endVal
            return (endVal - startVal) * (t - 1) * t * t + endVal;
        }
    }
}
