namespace Primer
{
    public static class FloatExtensions
    {
        public static bool IsInteger(this float value) => value % 1 == 0;
        public static float GetDecimals(this float value) => value % 1;
    }
}
