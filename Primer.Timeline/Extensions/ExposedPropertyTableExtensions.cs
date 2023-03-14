using UnityEngine;

namespace Primer.Timeline
{
    public static class ExposedPropertyTableExtensions
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

        public static T Get<T>(this IExposedPropertyTable resolver, NoBullshitExposedReference<T> reference) where T : Object
        {
            return reference.exposedReference.Resolve(resolver);
        }

        public static void Set<T>(this IExposedPropertyTable resolver, NoBullshitExposedReference<T> reference, T value)
            where T : Object
        {
            resolver.SetReferenceValue(reference.exposedReference.exposedName, value);
        }
    }
}
