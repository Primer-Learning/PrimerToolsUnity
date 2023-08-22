using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    /// <summary>
    ///   Extension of <code>PrimerPlayable</code> that includes <code>TTrackBind trackTarget</code> property
    ///   containing the Component is bound to the track of this playable
    ///   this is set before <code>Start()</code> is invoked so it's safe to use
    ///   from <code>Start()</code>, <code>Stop()</code> and after invoking <code>base.ProcessFrame()</code>
    /// </summary>
    /// <typeparam name="TTrackBind">Type of the object expected to be bound to the track</typeparam>
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
