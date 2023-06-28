using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Primer
{
    public static class ArrayExtensions
    {
        public static void Deconstruct<T>(this IEnumerable<T> self, out T first)
        {
            var list = self.ToList();
            first = list[0];
        }
        public static void Deconstruct<T>(this IEnumerable<T> self, out T first, out T second)
        {
            var list = self.ToList();
            first = list[0];
            second = list[1];
        }
        public static void Deconstruct<T>(this IEnumerable<T> self, out T first, out T second, out T third)
        {
            var list = self.ToList();
            first = list[0];
            second = list[1];
            third = list[2];
        }

        [Pure]
        public static TSource MaxBy<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var list = source.ToList();

            if (list.Count is 0)
                return default(TSource);

            var max = list[0];
            var maxVal = selector(max);

            foreach (var item in list.Skip(1)) {
                var key = selector(item);

                if (key > maxVal) {
                    max = item;
                    maxVal = key;
                }
            }

            return max;
        }

        [Pure]
        public static bool IsSame<T>(this IEnumerable<T> left, IEnumerable<T> right)
        {
            if (left is null && right is null)
                return true;

            if (left is null || right is null)
                return false;

            return left.SequenceEqual(right);
        }

        [Pure]
        public static T[] Append<T>(this T[] self, params T[] other) {
            var result = new T[self.Length + other.Length];
            self.CopyTo(result, 0);
            other.CopyTo(result, self.Length);
            return result;
        }

        [Pure]
        public static IEnumerable<(int index, T value)> WithIndex<T>(this IEnumerable<T> items)
        {
            return items.Select((value, i) => (i, value));
        }

        [Pure]
        public static IEnumerable<(T, float)> SpaceEvenly<T>(this IEnumerable<T> items, float distanceBetweenChildren)
        {
            var children = items.ToArray();

            for (var i = 0; i < children.Length; i++) {
                var y = GetEvenlySpacedFloatGivenStep(i, children.Length, distanceBetweenChildren);
                yield return (children[i], y);
            }
        }

        private static float GetEvenlySpacedFloatGivenStep(int objectIndex, int totalObjects, float distanceBetweenObjects)
        {
            var totalSpace = distanceBetweenObjects * (totalObjects - 1);
            var min = -totalSpace / 2;
            return min + objectIndex * distanceBetweenObjects;
        }
    }
}
