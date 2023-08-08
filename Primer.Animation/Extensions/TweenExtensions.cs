namespace Primer.Animation
{
    public static class TweenExtensions
    {
        public static bool IsEmpty(this Tween self)
        {
            return self is null || self == Tween.noop;
        }

        /// <summary>
        ///   This is an extension method so it can be used over null values
        /// </summary>
        public static Tween WithName(this Tween self, string name)
        {
            return (self ?? Tween.noop) with { name = name };
        }

        /// <summary>
        ///   This is an extension method so it can be used over null values
        /// </summary>
        public static Tween WithDuration(this Tween self, float duration)
        {
            return self == null ? null : self with { duration = duration };
        }
    }
}
