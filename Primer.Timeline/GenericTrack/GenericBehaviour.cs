using System;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    public class GenericBehaviour : PrimerPlayable<Transform>
    {
        /// <summary>Duration of the clip, not considering any extrapolation.</summary>
        /// <remarks><c>playable.GetDuration()</c> includes the extrapolated time as well, so may be infinity.</remarks>
        [HideInInspector]
        public float start;
        [HideInInspector]
        public float duration;
        public float end => start + duration;

        public string clipName => playableName ?? "Generic Clip";
        public virtual string playableName => null;

        public virtual void Execute(float time) {}
    }
}
