namespace Primer
{
    public abstract class Singleton<T> where T : new()
    {
        private static T instanceCache;
        public static T instance => instanceCache ??= new T();
    }
}
