using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class TriggerablePlayable : GenericBehaviour
    {
        [SerializeReference]
        [ValueDropdown(nameof(GetTriggerableOptions))]
        [Tooltip("Components in the track's target that extend TriggeredBehaviour")]
        internal Triggerable triggerable;

        [SerializeField]
        [ShowIf(nameof(triggerable))]
        [MethodOf(nameof(triggerable))]
        [Tooltip("Method to call when the clip is played")]
        internal MethodInvocation triggerMethod;


        public override char icon => '╬';
        public override string playableName => triggerMethod.ToString(triggerable);


        #region Triggerable management
        private static HashSet<Triggerable> initializationTracker = new();

        public void Prepare()
        {
            if (triggerable == null || initializationTracker.Contains(triggerable))
                return;

            triggerable.Prepare();
            initializationTracker.Add(triggerable);
        }

        public void Cleanup()
        {
            if (triggerable == null || !initializationTracker.Contains(triggerable))
                return;

            triggerable.Cleanup();
            initializationTracker.Remove(triggerable);
        }

        public void Execute(float time)
        {
            if (triggerable is null) {
                Debug.LogWarning($"[{this}] no triggerable selected.\nIf no triggerable is available consider adding a {nameof(Triggerable)} to the track's target");
                return;
            }

            triggerMethod.Invoke(triggerable);
        }
        #endregion


        public override string ToString()
        {
            return $"Trigger {icon} {triggerMethod.ToString(triggerable)}";
        }

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (triggerable is null && trackTarget != null)
                triggerable = trackTarget.GetComponent<Triggerable>();
        }

        internal Triggerable[] GetTriggerableOptions()
        {
            return trackTarget == null
                ? Array.Empty<Triggerable>()
                : trackTarget.GetComponents<Triggerable>();
        }
    }
}
