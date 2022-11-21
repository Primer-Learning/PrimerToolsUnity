using UnityEngine;

namespace Primer
{
    public static class FloatExtensions
    {
        public static bool IsInteger(this float value) => value % 1 == 0;
        public static float GetDecimals(this float value) => value % 1;

        public static float Remap(this float value, float aLow, float aHigh, float bLow, float bHigh) {
            var normal = Mathf.InverseLerp(aLow, aHigh, value);
            return Mathf.Lerp(bLow, bHigh, normal);
        }
    }
}
