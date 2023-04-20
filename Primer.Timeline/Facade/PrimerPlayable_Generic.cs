using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrimerPlayable<TTrackBind> : PrimerPlayable
        where TTrackBind : Component
    {
        private TTrackBind _trackTarget;

        public TTrackBind trackTarget {
            get {
                if (_trackTarget == null && trackTransform != null)
                    _trackTarget = trackTransform.GetComponent<TTrackBind>();

                return _trackTarget;
            }
            internal set {
                _trackTarget = value;
                trackTransform = value == null ? null : value.transform;
            }
        }


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            using (lifecycle.PreventInitialization())
                base.ProcessFrame(playable, info, playerData);

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
