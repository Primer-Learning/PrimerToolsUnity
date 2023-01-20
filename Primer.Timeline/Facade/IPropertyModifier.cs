using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public interface IPropertyModifier
    {
        /// <summary>Registers all properties that may be modified by Update.</summary>
        /// <remarks>
        ///     This ensures changes made in preview mode don't get saved to the scene, so it's important
        ///     that all properties that may be modified are registered here.
        /// </remarks>
        void RegisterProperties(IPropertyCollector registrar);
    }
}
