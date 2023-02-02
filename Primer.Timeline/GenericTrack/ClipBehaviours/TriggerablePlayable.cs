using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        [MethodOf(nameof(triggerable), parameters = new Type[] {}, excludeNames = new[] { "Prepare", "Cleanup" })]
        [Tooltip("Method to call when the clip is played")]
        internal MethodInvocation triggerMethod;


        private static Regex removeStep = new(@"^Step(\d+)", RegexOptions.Compiled);

        static TriggerablePlayable() => SetIcon<TriggerablePlayable>('╬');

        public override string playableName
            => triggerable == null
                ? "No triggerable selected"
                : removeStep.Replace(triggerMethod.ToString(), "$1");


        #region Triggerable management
        private static HashSet<Triggerable> cleannessTracker = new();

        public void Prepare()
        {
            if (triggerable == null || !cleannessTracker.Contains(triggerable))
                return;

            triggerable.Prepare();
            cleannessTracker.Remove(triggerable);
        }

        public void Cleanup()
        {
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

            Prepare();
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
