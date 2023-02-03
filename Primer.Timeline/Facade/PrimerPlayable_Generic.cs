using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerPlayable<TTrackBind> : PrimerPlayable
        where TTrackBind : Component
    {
        public virtual TTrackBind trackTarget { get; internal set; }


        private ExposedReferencesTable referenceResolverCache;
        protected ExposedReferencesTable referenceResolver {
            get {

                if (referenceResolverCache != null) {
                    return referenceResolverCache;
                }

                if (trackTarget == null)
                    return null;

                referenceResolverCache = trackTarget.GetExposedReferencesResolver();

                if (referenceResolverCache == null)
                    throw new Exception($"No exposed references resolver found");

                return referenceResolverCache;
            }
        }


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            using (lifecycle.PreventInitialization()) {
                base.ProcessFrame(playable, info, playerData);
            }

            if (playerData is null)
                return;

            if (playerData is not TTrackBind boundObject || boundObject is null) {
                throw new Exception(
                    $"Expected track target to be {typeof(TTrackBind).Name} but {playerData?.GetType().Name} found"
                );
            }

            trackTarget = boundObject;
            lifecycle.Initialize();
        }
    }
}
