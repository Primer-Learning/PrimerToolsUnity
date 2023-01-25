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
        [HideInInspector]
        public double duration;

        [SerializeReference]
        [Required]
        [HideLabel]
        [InlineProperty]
        [Title("Animation", "Extend Scrubbable class to add more types")]
        public Scrubbable animation;

        [SerializeField]
        [HideInInspector]
        internal string method;


        public string clipName => animation is null ? "Generic Clip" : animation.GetType().Name;

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

            var t = (float)(playable.GetTime() / duration);

            if (method is null || method == "Update") {
                animation.Update(t);
                return;
            }

            var methodInfo = animation.GetType().GetMethod(method);

            if (methodInfo is null)
                throw new Exception($"No method {method}(float t) found in ${animation.GetType().FullName}");

            methodInfo.Invoke(animation, new object[] { t });
        }
    }
}
