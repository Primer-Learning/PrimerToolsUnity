using System.Collections.Generic;
using System.Linq;

namespace Primer
{
    public static class IntExtensions
    {
        public static bool IsSame(this IEnumerable<int> left, IEnumerable<int> right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.SequenceEqual(right);
        }
    }
}
