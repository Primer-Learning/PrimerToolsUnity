using UnityEngine;

namespace Primer
{
    public enum EaseMode
    {
        Cubic,
        Quadratic,
        CubicIn,
        CubicOut,
        SmoothStep, //Built-in Unity function that's mostly linear but smooths edges
        DoubleSmoothStep,
        SmoothIn,
        SmoothOut,
        None
    }


    public static class Easing
    {
        public static float ApplyNormalizedEasing(float t, EaseMode ease) {
            switch (ease) {
                case EaseMode.Cubic:
                    return easeInAndOutCubic(0, 1, t);
                case EaseMode.Quadratic:
                    return easeInAndOutQuadratic(0, 1, t);
                case EaseMode.CubicIn:
                    return easeInCubic(0, 1, t);
                case EaseMode.CubicOut:
                    return easeOutCubic(0, 1, t);
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

        static float easeInAndOutCubic(float startVal, float endVal, float t) {
            // Scale time relative to half duration
            t *= 2;

            // handle different
            if (t <= 0) return startVal;

            if (t <= 1) {
                // Ease in from startVal to half the overall change
                return (endVal - startVal) / 2 * t * t * t + startVal;
            }

            if (t <= 2) {
                // Make t negative to use left side of cubic
                t -= 2;
                // Ease out from half to end of overall change
                return (endVal - startVal) / 2 * t * t * t + endVal;
            }

            return endVal;
        }

        static float easeInCubic(float startVal, float endVal, float t) {
            // Ease in from startVal
            return (endVal - startVal) * t * t * t + startVal;
        }

        static float easeOutCubic(float startVal, float endVal, float t) {
            // Make t negative to use left side of cubic
            t -= 1;
            // Ease out from half to end of overall change
            return (endVal - startVal) * t * t * t + endVal;
        }

        static float easeInAndOutQuadratic(float startVal, float endVal, float t) {
            // Scale time relative to half duration
            t *= 2;

            // handle different
            if (t <= 0) return startVal;

            if (t <= 1) {
                //Ease in from zero to half the overall change
                return (endVal - startVal) / 2 * t * t + startVal;
            }

            if (t <= 2) {
                t -= 2; //Make t negative to use other left side of quadratic
                //Ease out from half to end of overall change
                return -(endVal - startVal) / 2 * t * t + endVal;
            }

            return endVal;
        }
    }
}
