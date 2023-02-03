using UnityEngine;

namespace Primer.Timeline
{
    public static class ComponentExtensions
    {
        public static ExposedReferencesTable GetExposedReferencesResolver(this Component transform)
        {
            if (transform == null)
                return null;

            return transform.gameObject.GetOrAddComponent<ExposedReferencesTable>();
        }
    }
}
