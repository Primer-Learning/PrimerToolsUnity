using UnityEngine;

namespace Primer
{
    public static class GameObjectExtensions
    {
        /// <summary>Will be true if game object is preset.</summary>
        /// <remarks>
        ///     This condition was found through exploration... There is no documented way to determine
        ///     whether we're currently editing a preset. There's likely to be other cases where this is true
        ///     that we'll want to figure out how to exclude. But we'll handle those as needed.
        /// </remarks>
        public static bool IsPreset(this GameObject gameObject)
        {
            return gameObject.scene.handle == 0 || gameObject.scene.path == "";
        }

        public static PrimerComponent GetPrimer(this GameObject gameObject)
        {
            return gameObject.GetOrAddComponent<PrimerComponent>();
        }
    }
}
