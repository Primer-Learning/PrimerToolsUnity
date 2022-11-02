using System;
using UnityEngine;

namespace Primer
{
    public static class Presentation
    {
        public static string FormatNumber(float number) {
            return $"{number:N0}";
        }

        public static string FormatNumberWithDecimals(float number) {
            return $"{number:#.##}";
        }
    }
}
