using System;
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
        internal TriggeredBehaviour triggerable;

        [SerializeField]
        [ShowIf(nameof(triggerable))]
        [MethodOf(nameof(triggerable))]
        [Tooltip("Method to call when the clip is played")]
        internal MethodInvocation triggerMethod;


        public override string icon => "╬";
        public override string playableName => triggerMethod.ToString(triggerable);


        public void Execute(float time)
        {
            if (triggerable is null) {
                Debug.LogWarning($"[{this}] no triggerable selected.\nIf no triggerable is available consider adding a {nameof(TriggeredBehaviour)} to the track's target");
                return;
            }

            triggerMethod.Invoke(triggerable);
        }

        public override string ToString()
        {
            return $"Trigger {icon} {triggerMethod.ToString(triggerable)}";
        }

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (triggerable is null && trackTarget != null)
                triggerable = trackTarget.GetComponent<TriggeredBehaviour>();
        }

        internal TriggeredBehaviour[] GetTriggerableOptions()
        {
            return trackTarget == null
                ? Array.Empty<TriggeredBehaviour>()
                : trackTarget.GetComponents<TriggeredBehaviour>();
        }
    }
}
