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
    }
}
