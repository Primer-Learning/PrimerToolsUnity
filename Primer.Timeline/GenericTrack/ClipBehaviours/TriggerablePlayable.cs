using System;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class TriggerablePlayable : GenericBehaviour, IEquatable<TriggerablePlayable>
    {
        [SerializeField, HideInInspector]
        internal NoBullshitExposedReference<Triggerable> _triggerable = "TriggerablePlayable";

        [ShowInInspector]
        [PropertyOrder(1)]
        [ValueDropdown(nameof(GetTriggerableOptions))]
        [Tooltip("Components in the track's target that extend Triggerable")]
        public Triggerable triggerable {
            get => resolver.Get(_triggerable);
            set => resolver.Set(_triggerable, value);
        }

        [SerializeField]
        [PropertyOrder(2)]
        [ShowIf(nameof(triggerable))]
        [MethodOf(nameof(triggerable), parameters = new Type[] {}, excludeNames = new[] { "Prepare", "Cleanup" })]
        [Tooltip("Method to call when the clip is played")]
        internal MethodInvocation triggerMethod;


        public override Transform trackTarget {
            get => base.trackTarget;
            internal set {
                base.trackTarget = value;

                if (value == null)
                    triggerable = null;
                else if (triggerable is null || triggerable.gameObject != value.gameObject)
                    triggerable = value.GetComponent<Triggerable>();
            }
        }


        #region Clip name
        private static Regex removeStep = new(@"^Step(\d+)", RegexOptions.Compiled);

        static TriggerablePlayable() => SetIcon<TriggerablePlayable>('╬');

        public override string playableName
            => triggerable == null
                ? "No triggerable selected"
                : removeStep.Replace(triggerMethod.ToString(), "$1");
        #endregion


        #region Triggerable management
        private static PlayableTracker<Triggerable> tracker = new();
        private bool isExecuted = false;

        public void Prepare()
        {
            if (!tracker.IsPrepared(triggerable))
                triggerable.Prepare();
        }

        public void Cleanup()
        {
            isExecuted = false;

            if (!tracker.IsClean(triggerable))
                triggerable.Cleanup();
        }

        public void Execute(float time)
        {
            if (triggerable is null) {
                Debug.LogWarning($"[{this}] no triggerable selected.\nIf no triggerable is available consider adding a {nameof(Triggerable)} to the track's target");
                return;
            }

            if (isExecuted)
                return;

            Prepare();
            triggerMethod.Invoke(triggerable);
            isExecuted = true;
        }
        #endregion


        public override string ToString()
        {
            return $"Trigger {icon} {triggerMethod.ToString(triggerable)}";
        }

        internal Triggerable[] GetTriggerableOptions()
        {
            return trackTarget == null
                ? Array.Empty<Triggerable>()
                : trackTarget.GetComponents<Triggerable>();
        }


        public bool Equals(TriggerablePlayable other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Equals(triggerable, other.triggerable) && triggerMethod.Equals(other.triggerMethod);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj.GetType() != GetType())
                return false;

            return Equals((TriggerablePlayable)obj);
        }

        public override int GetHashCode() => HashCode.Combine(triggerable, triggerMethod);
    }
}
