namespace Primer
{
    public static class Presentation
    {
        public static string FormatNumber(float number)
            => $"{number:N0}";

        public static string FormatNumberWithDecimals(float number)
            => number == 0 ? "0" : $"{number:#.##}";
    }
}
