using UnityEngine;

namespace Primer
{
    // From https://youtu.be/R6UB7mVO3fY?t=1462
    // thanks @acegikmo
    public static class PrimerMath
    {
        public static float Remap(float iMin, float iMax, float oMin, float oMax, float v)
        {
            var t = Mathf.InverseLerp(iMin, iMax, v);
            return Mathf.Lerp(oMin, oMax, t);
        }
    }
}
