using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Serializable]
    public class GenericBehaviour : PrimerPlayable<Transform>
    {
        public bool isFailed = false;
        public Scrubbable<Transform> animation;

        /// <summary>Duration of the clip, not considering any extrapolation.</summary>
        /// <remarks><tt>playable.GetDuration()</tt> includes the extrapolated time as well, so may be infinity.</remarks>
        public double duration;

        protected override void Start() => animation.Prepare();
        protected override void Stop() => animation.Cleanup();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);

            if (isFailed)
                return;

            var t = playable.GetTime() / duration;
            animation.Update((float)t);
        }

    }
}
