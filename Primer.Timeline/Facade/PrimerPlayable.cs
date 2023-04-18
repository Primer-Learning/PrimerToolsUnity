using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Serializable]
    public class PrimerPlayable : PlayableBehaviour
    {
        private Lifecycle _lifecycle;
        protected Lifecycle lifecycle => _lifecycle ??= new Lifecycle(Start, Stop);


        protected virtual void Start() {}

        protected virtual void Stop() {}


        public override void OnBehaviourPause(Playable playable, FrameData info) => lifecycle.Reset();
        public override void OnPlayableDestroy(Playable playable) => lifecycle.Reset();
        public override void OnGraphStop(Playable playable) => lifecycle.Reset();


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            lifecycle.Initialize();
        }


        #region Values to be set by the mixer
        /// <summary>Duration of the clip, not considering any extrapolation.</summary>
        /// <remarks><c>playable.GetDuration()</c> includes the extrapolated time as well, so may be infinity.</remarks>
        [HideInInspector] public float start;
        [HideInInspector] public float duration;
        public float end => start + duration;

        [HideInInspector] public int clipIndex;
        [HideInInspector] public float weight;

        public Transform trackTransform;

        private IExposedPropertyTable _resolver;
        internal IExposedPropertyTable resolver {
            get {
                if (_resolver is null)
                    return null;

                // HACK: This is the only way we could make sure the resolver is still valid.
                // _resolver == null returns false
                try {
                    _resolver.ClearReferenceValue(default(PropertyName));
                }
                catch (Exception) {
                    _resolver = null;
                }

                return _resolver;
            }
            set => _resolver = value;
        }

        public Action<float> onDurationReported;

        public void ReportDuration(float newDuration)
        {
            onDurationReported?.Invoke(newDuration);
        }
        #endregion
    }
}
