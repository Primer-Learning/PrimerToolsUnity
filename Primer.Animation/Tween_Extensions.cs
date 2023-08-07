namespace Primer.Animation
{
    public static class Tween_Extensions
    {
        /// <summary>
        ///   This is an extension method so it can be used over null values
        /// </summary>
        public static Tween WithDuration(this Tween self, float duration)
        {
            return self == null ? null : self with { duration = duration };
        }
    }
}
