using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Primer
{
    public static class FloatExtensions
    {
        public static bool IsInteger(this float value) => value % 1 == 0;
        public static float GetDecimals(this float value) => value % 1;


        public static string FormatRoundedToInt(this float number) => $"{number:N0}";
        public static string FormatNumberWithDecimals(this float number) => number == 0 ? "0" : $"{number:0.##}";


        public static float Remap(this float value, float aLow, float aHigh, float bLow, float bHigh) {
            return PrimerMath.Remap(aLow, aHigh, bLow, bHigh, value);
        }

        public static IEnumerable<float> ToFloats(this IEnumerable<int> self)
        {
            return self.Select<int, float>(i => i);
        }

        public static float[] ToFloatArray(this IEnumerable<int> self)
        {
            return self.ToFloats().ToArray();
        }

        public static string FormatRoundedToIntForLatexApproxIfRoundsToZero(this float number)
        {
            var formatted = number.FormatRoundedToInt();
            
            // This method should only change the formatting if 
            if (formatted != "0" || number == 0)
                return formatted;
            
            return @"\sim" + formatted;
        }
        public static string FormatSignificantDigits(this float number, int significantDigits, int? maxDigitsAfterDecimalPoint = null)
        {
            if (number == 0)
            {
                return "0";
            }

            // Determine if decimal point is needed.
            float scale = Mathf.Floor(Mathf.Log10(Mathf.Abs(number)));
            int decimalPlacesNeeded = significantDigits - (int)(scale + 1);
    
            // Ensure decimalPlacesNeeded isn't negative.
            decimalPlacesNeeded = Mathf.Max(0, decimalPlacesNeeded); 

            // Check if maxDigitsAfterDecimalPoint is set and less than decimalPlacesNeeded
            bool maxDigitsLimitReached = maxDigitsAfterDecimalPoint.HasValue && maxDigitsAfterDecimalPoint.Value < decimalPlacesNeeded;
    
            if (maxDigitsLimitReached)
            {
                decimalPlacesNeeded = maxDigitsAfterDecimalPoint.Value;
            }

            string format = "{0:F" + decimalPlacesNeeded + "}";
            string result = string.Format(format, number);

            // Prepend "~" if maxDigitsLimitReached
            if (maxDigitsLimitReached)
            {
                result = "~" + result;
            }

            return result;
        }
    }
}
