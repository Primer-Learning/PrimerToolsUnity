using UnityEngine;

namespace Primer.Animation
{
    public enum EaseMode
    {
        Cubic,
        Quadratic,
        CubicIn,
        CubicOut,
        /// <summary>Built-in Unity function that's mostly linear but smooths edges</summary>
        SmoothStep,
        DoubleSmoothStep,
        SmoothIn,
        SmoothOut,
        None,
    }

	public static class EasingMethods {
        public static float Apply(this EaseMode ease, float t) {
            switch (ease) {
                case EaseMode.Cubic:
                    return EaseInAndOutCubic(0, 1, t);
                case EaseMode.Quadratic:
                    return EaseInAndOutQuadratic(0, 1, t);
                case EaseMode.CubicIn:
                    return EaseInCubic(0, 1, t);
                case EaseMode.CubicOut:
                    return EaseOutCubic(0, 1, t);
                case EaseMode.SmoothStep:
                    return Mathf.SmoothStep(0, 1, t);
                case EaseMode.DoubleSmoothStep:
                    return (Mathf.SmoothStep(0, 1, t) + t) / 2;
                case EaseMode.SmoothIn:
                    // Stretch the function and just use first half
                    return Mathf.SmoothStep(0, 2, t / 2);
                case EaseMode.SmoothOut:
                    // Stretch the function and just use second half
                    // return t;
                    return Mathf.SmoothStep(0, 2, (t + 1) / 2) - 1;
                case EaseMode.None:
                default:
                    return t;
            }
        }

        private static float EaseInAndOutCubic(float startVal, float endVal, float t) {
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

        private static float EaseInCubic(float startVal, float endVal, float t) {
            // Ease in from startVal
            return (endVal - startVal) * t * t * t + startVal;
        }

        private static float EaseOutCubic(float startVal, float endVal, float t) {
            // Make t negative to use left side of cubic
            t -= 1;
            // Ease out from half to end of overall change
            return (endVal - startVal) * t * t * t + endVal;
        }

        private static float EaseInAndOutQuadratic(float startVal, float endVal, float t) {
            // Scale time relative to half duration
            t *= 2;

            switch (t) {
                // handle different
                case <= 0:
                    return startVal;

                case <= 1:
                    //Ease in from zero to half the overall change
                    return (endVal - startVal) / 2 * t * t + startVal;

                case <= 2:
                    t -= 2; //Make t negative to use other left side of quadratic
                    //Ease out from half to end of overall change
                    return -(endVal - startVal) / 2 * t * t + endVal;

                default:
                    return endVal;
            }
        }
	}
}
