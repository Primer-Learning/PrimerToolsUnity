using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class TriggerablePlayable : GenericBehaviour, IEquatable<TriggerablePlayable>
    {
        [SerializeField]
        [HideInInspector]
        private ExposedReference<Triggerable> _triggerable;

        [ShowInInspector]
        [PropertyOrder(1)]
        [ValueDropdown(nameof(GetTriggerableOptions))]
        [Tooltip("Components in the track's target that extend Triggerable")]
        internal Triggerable triggerable {
            get => referenceResolver.Get(_triggerable);
            set => referenceResolver.Set(_triggerable, value);
        }

        [SerializeField]
        [PropertyOrder(2)]
        [ShowIf(nameof(triggerable))]
        [MethodOf(nameof(triggerable), parameters = new Type[] {}, excludeNames = new[] { "Prepare", "Cleanup" })]
        [Tooltip("Method to call when the clip is played")]
        internal MethodInvocation triggerMethod;


        #region Clip name
        private static Regex removeStep = new(@"^Step(\d+)", RegexOptions.Compiled);

        static TriggerablePlayable() => SetIcon<TriggerablePlayable>('╬');

        public override string playableName {
            get {
                if (triggerable == null)
                    Debug.Log("No triggerable selected");

                // PrimerLogger.Log(
                //     "TriggerablePlayable.playableName",
                //     triggerable,
                //     triggerMethod,
                //     (this as IExposedReferenceResolver).resolver
                // );
                return triggerable == null
                    ? "No triggerable selected"
                    : removeStep.Replace(triggerMethod.ToString(), "$1");
            }
        }
        #endregion


        #region Triggerable management
        private static HashSet<Triggerable> cleannessTracker = new();
        private bool isExecuted = false;

        public void Prepare()
        {
            if (triggerable == null || !cleannessTracker.Contains(triggerable))
                return;

            triggerable.Prepare();
            cleannessTracker.Remove(triggerable);
        }

        public void Cleanup()
        {
            isExecuted = false;

            if (triggerable == null || cleannessTracker.Contains(triggerable))
                return;

            triggerable.Cleanup();
            cleannessTracker.Add(triggerable);
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

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (triggerable is null && trackTarget != null) {
                PrimerLogger.Log("Filling", triggerable, trackTarget.GetComponent<Triggerable>());
                triggerable = trackTarget.GetComponent<Triggerable>();
            }
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
