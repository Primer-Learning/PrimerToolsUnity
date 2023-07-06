using UnityEngine;

namespace Primer
{
    public static class IPrimer_FindOrCreateExtensions
    {
        public static Transform FindOrCreate(this IPrimer self, string name, ChildOptions options = null)
        {
            options ??= new ChildOptions();
            return self.ToContainer().Add(name, options with { ignoreSiblingOrder = true });
        }

        public static Transform FindOrCreate(this IPrimer self, ref Transform cache, string name,
            ChildOptions options = null)
        {
            if (cache == null)
                cache = self.FindOrCreate(name, options);

            return cache;
        }

        public static T FindOrCreate<T>(this IPrimer self, string name, ChildOptions options = null)
            where T : Component
        {
            options ??= new ChildOptions();
            return self.ToContainer().Add<T>(name, options with { ignoreSiblingOrder = true });
        }

        public static T FindOrCreate<T>(this IPrimer self, ref T cache, string name, ChildOptions options = null)
            where T : Component
        {
            if (cache == null)
                cache = self.FindOrCreate<T>(name, options);

            return cache;
        }

        public static Transform FindOrCreate(this Component self, string name, ChildOptions options = null)
        {
            options ??= new ChildOptions();
            return self.ToContainer().Add(name, options with { ignoreSiblingOrder = true });
        }

        public static Transform FindOrCreate(this Component self, ref Transform cache, string name,
            ChildOptions options = null)
        {
            if (cache == null)
                cache = self.FindOrCreate(name, options);

            return cache;
        }

        public static T FindOrCreate<T>(this Component self, string name, ChildOptions options = null)
            where T : Component
        {
            options ??= new ChildOptions();
            return self.ToContainer().Add<T>(name, options with { ignoreSiblingOrder = true });
        }

        public static T FindOrCreate<T>(this Component self, ref T cache, string name, ChildOptions options = null)
            where T : Component
        {
            if (cache == null)
                cache = self.FindOrCreate<T>(name, options);

            return cache;
        }
    }
}
