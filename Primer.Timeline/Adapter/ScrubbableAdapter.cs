using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer.Timeline
{
    [Serializable]
    public abstract class ScrubbableAdapter
    {
        [NonSerialized] internal List<Exception> errors = new();
        [NonSerialized] internal IExposedPropertyTable resolver;

        /// <summary>Updates the game state each frame while the clip is playing.</summary>
        public abstract void Update(UpdateArgs args);

        /// <summary>Called just before the timeline starts playing.</summary>
        /// <remarks>
        ///     Scene objects will have their serialized values when this is called. Capture the original
        ///     values of any properties you modify here so you can restore them in <tt>Cleanup</tt>.
        /// </remarks>
        public abstract void Prepare();

        /// <summary>Called just after the adapter stops playing.</summary>
        /// <remarks>
        ///     Scene objects should be reset back to their original values when this is called. If the
        ///     post-extrapolation mode is hold, this will only be called when another clip starts playing on
        ///     the same track (or we're exiting preview mode in the editor).
        /// </remarks>
        public abstract void Cleanup();

        /// <summary>Registers all properties that may be modified by Update.</summary>
        /// <remarks>
        ///     This ensures changes made in preview mode don't get saved to the scene, so it's important
        ///     that all properties that may be modified are registered here.
        /// </remarks>
        public abstract void RegisterPreviewingProperties(PropertyRegistrar registrar);

        /// <summary>Follows an ExposedReference and gets the object in the scene it points to.</summary>
        protected T Resolve<T>(ExposedReference<T> exposedReference) where T : Object => exposedReference.Resolve(resolver);

        /// <summary>Arguments given to Update.</summary>
        /// <remarks>
        ///     This is a struct (rather than just parameters on the function) to make
        ///     backwards-compatibility easier when the parameters change.
        /// </remarks>
        public struct UpdateArgs
        {
            /// <summary>The current time relative to the clips start and duration. Always between 0 and 1.</summary>
            public double time;
        }
    }
}
