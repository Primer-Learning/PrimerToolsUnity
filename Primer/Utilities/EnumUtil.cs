using System;

namespace Primer
{
    public static class EnumUtil
    {
        public static T[] Values<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
