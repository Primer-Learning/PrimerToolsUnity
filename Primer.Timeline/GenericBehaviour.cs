using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [Serializable]
    public class GenericBehaviour : PrimerPlayable<Transform>
    {
        /// <summary>Duration of the clip, not considering any extrapolation.</summary>
        /// <remarks><c>playable.GetDuration()</c> includes the extrapolated time as well, so may be infinity.</remarks>
        public double duration;

        [SerializeReference]
        [Required]
        [HideLabel]
        [InlineProperty]
        [Title("Animation", "Extend Scrubbable class to add more types")]
        public Scrubbable animation;


        protected override Transform trackTarget {
            get => animation?.target;
            set {
                if (animation is not null)
                    animation.target = value;
            }
        }


        protected override void Start() => animation?.Prepare();
        protected override void Stop() => animation?.Cleanup();


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (animation is null)
                return;

            base.ProcessFrame(playable, info, playerData);

            if (isFailed)
                return;

            var t = playable.GetTime() / duration;
            animation.Update((float)t);
        }
    }
}
