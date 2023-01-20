using UnityEngine;

namespace Primer.Timeline
{
    public interface IExposedReferenceResolver
    {
        internal IExposedPropertyTable resolver { get; set; }
    }

    public static class ExposedReferenceResolverExtensions
    {
        public static T Resolve<T>(
            this IExposedReferenceResolver self,
            ExposedReference<T> reference)
            where T : Object
        {
            return reference.Resolve(self.resolver);
        }
    }
}
