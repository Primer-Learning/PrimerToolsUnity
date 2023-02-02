using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    public class GenericBehaviour : PrimerPlayable<Transform>
    {
        #region Values to be set by the mixer
        [HideInInspector]
        public float weight;

        /// <summary>Duration of the clip, not considering any extrapolation.</summary>
        /// <remarks><c>playable.GetDuration()</c> includes the extrapolated time as well, so may be infinity.</remarks>
        [HideInInspector]
        public float start;

        [HideInInspector]
        public float duration;

        public float end => start + duration;
        #endregion


        #region Clip name customisation
        public string clipName {
            get {
                knownIcons.Add(icon);
                var name = playableName;
                return name == null ? "Generic Clip" : $"{icon} {name}";
            }
        }

        private static HashSet<char> knownIcons = new();

        public static bool IsGeneratedClipName(string value)
        {
            return value[1] == ' ' && knownIcons.Contains(value[0]);
        }
        #endregion


        // Following members are "abstract" in the sense that they are not implemented here, but are implemented in the derived classes.
        // we can't use the "abstract" keyword because Unity's Timeline API requires a default constructor in all PlayableBehaviours.

        public virtual char icon { get; }
        public virtual string playableName { get; }
    }
}
