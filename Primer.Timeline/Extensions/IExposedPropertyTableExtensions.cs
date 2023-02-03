using UnityEngine;

namespace Primer.Timeline
{
    public static class IExposedPropertyTableExtensions
    {
        public static T Get<T>(this IExposedPropertyTable resolver, ExposedReference<T> reference) where T : Object
        {
            return reference.Resolve(resolver);
        }

        public static void Set<T>(this IExposedPropertyTable resolver, ExposedReference<T> reference, T value)
            where T : Object
        {
            resolver.SetReferenceValue(reference.exposedName, value);
        }
    }
}
