using System.Collections.Generic;
using System.Linq;

namespace Primer
{
    public static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] self, params T[] other) {
            var result = new T[self.Length + other.Length];
            self.CopyTo(result, 0);
            other.CopyTo(result, self.Length);
            return result;
        }

        public static IEnumerable<float> ToFloats(this IEnumerable<int> self)
        {
            return self.Select<int, float>(i => i);
        }

        public static IEnumerable<float> ToFloatArray(this IEnumerable<int> self)
        {
            return self.ToFloats().ToArray();
        }
    }
}
