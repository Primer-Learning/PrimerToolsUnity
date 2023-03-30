namespace Primer
{
    // From https://youtu.be/R6UB7mVO3fY?t=1462
    // thanks @acegikmo
    public static class PrimerMath
    {
        public static float Lerp(float a, float b, float t)
        {
            return (1f - t) * a + b * t;
        }

        public static float InvLerp(float a, float b, float v)
        {
            return (v - a) / (b - a);
        }

        public static float Remap(float iMin, float iMax, float oMin, float oMax, float v)
        {
            var t = InvLerp(iMin, iMax, v);
            return Lerp(oMin, oMax, t);
        }
    }
}
