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
        #endregion


        #region Clip name customization
        private const string DEFAULT_CLIP_NAME = "Generic Clip";
        private static Dictionary<Type, char> knownIcons = new() {
            [typeof(GenericBehaviour)] = '-',
        };

        public string clipName {
            get {
                var name = playableName;
                return name is null ? DEFAULT_CLIP_NAME : $"{icon} {name}";
            }
        }

        public char icon {
            get {
                var type = GetType();
                return knownIcons[knownIcons.ContainsKey(type) ? type : typeof(GenericBehaviour)];
            }
        }

        protected static void SetIcon<T>(char icon) where T : GenericBehaviour
        {
            knownIcons.Add(typeof(T), icon);
        }

        public static bool IsGeneratedClipName(string value)
        {
            if (value.Length < 2)
                return false;

            if (value is nameof(GenericClip) or DEFAULT_CLIP_NAME)
                return true;

            return value[1] == ' ' && knownIcons.ContainsValue(value[0]);
        }
        #endregion


        #region Inform clip duration from the mixer
        public Action<float> onDurationReported;

        public void ReportDuration(float duration)
        {
            onDurationReported?.Invoke(duration);
        }
        #endregion


        // Following members are "abstract" in the sense that they are not implemented here, but are implemented in the derived classes.
        // we can't use the "abstract" keyword because Unity's Timeline API requires a default constructor in all PlayableBehaviours.

        public virtual string playableName { get; }
    }
}
