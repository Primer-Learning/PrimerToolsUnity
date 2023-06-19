using UnityEngine;

namespace Primer
{
    public static class IPrimer_FindOrCreate
    {
        public static Transform FindOrCreate(this IPrimer self, string name)
        {
            return self.ToContainer().Add(name);
        }

        public static T FindOrCreate<T>(this IPrimer self, string name) where T : Component
        {
            return self.ToContainer().Add<T>(name);
        }

        public static Transform FindOrCreate(this Component self, string name)
        {
            return self.ToContainer().Add(name);
        }

        public static T FindOrCreate<T>(this Component self, string name) where T : Component
        {
            return self.ToContainer().Add<T>(name);
        }
    }
}
