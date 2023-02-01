using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

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

        [Space(32)]
        [HideLabel]
        [EnumToggleButtons]
        public Kind kind = Kind.Scrubbable;
        public enum Kind { Scrubbable, Trigger, Sequence }

        #region Scrubbable
        [Space]
        [SerializeReference]
        [ShowIf("@kind == Kind.Scrubbable")]
        [TypeFilter(nameof(GetScrubbables))]
        [FormerlySerializedAs("animation")]
        [Tooltip("Extend Scrubbable class to add more options")]
        public Scrubbable scrubbable;

        [SerializeField]
        [MethodOf(nameof(scrubbable), parameters = new[] { typeof(float) })]
        [ShowIf("@kind == Kind.Scrubbable && scrubbable != null")]
        [Tooltip("Method to call when the clip is scrubbed. Only methods with a float parameter are shown here")]
        internal MethodInvocation scrubbableMethod;
        #endregion

        #region Trigger
        [Space]
        [SerializeField]
        [SerializeReference]
        [ShowIf("@kind == Kind.Trigger")]
        [ValueDropdown(nameof(GetTriggerableOptions))]
        [Tooltip("Components in the track's target that extend TriggeredBehaviour")]
        internal TriggeredBehaviour triggerable;

        [SerializeField]
        [SerializeReference]
        [MethodOf(nameof(triggerable))]
        [ShowIf("@kind == Kind.Trigger && triggerable != null")]
        [Tooltip("Method to call when the clip is played")]
        internal MethodInvocation triggerMethod;

        private ITriggeredBehaviour[] GetTriggerableOptions()
        {


            if (trackTarget == null) {
                PrimerLogger.Log("No track target");
                return Array.Empty<ITriggeredBehaviour>();
            }

            var list = trackTarget.GetComponents<ITriggeredBehaviour>();
            PrimerLogger.Log($"Triggerable {list.Length}");
            return list;
        }
        #endregion

        #region Sequence
        // [Space]
        // [SerializeField]
        // [SerializeReference]
        // [ShowIf("@kind == Kind.Sequence")]
        // [ValueDropdown(nameof(GetTriggerableOptions))]
        // internal Sequence sequence;
        //
        // [SerializeField]
        // [SerializeReference]
        // [ShowIf("@kind == Kind.Sequence && triggerable != null")]
        // [MethodOf(nameof(sequence))]
        // internal MethodInvocation sequenceMethod;
        //
        // private Sequence[] GetTriggerTarget() => trackTarget.GetComponents<Sequence>();
        #endregion

        public override Transform trackTarget {
            get => base.trackTarget;
            internal set {
                base.trackTarget = value;
                if (scrubbable is not null)
                    scrubbable.target = value;
            }
        }

        public string clipName => kind switch {
                Kind.Scrubbable => scrubbable?.GetType().Name,
                Kind.Trigger => triggerMethod?.ToString(),
                // Kind.Sequence => sequenceMethod?.ToString(),
                _ => null,
            }
            ?? "Generic Clip";

        protected override void Start()
        {
            scrubbable?.Prepare();
        }

        protected override void Stop()
        {
            scrubbable?.Cleanup();
        }

        public void Execute(float time)
        {
            switch (kind) {
                case Kind.Scrubbable:
                    ExecuteScrubbable(time);
                    break;

                case Kind.Trigger:
                    ExecuteTrigger();
                    break;

                case Kind.Sequence:
                    ExecuteSequence();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ExecuteScrubbable(float time)
        {
            if (triggerable is null) {
                Debug.LogWarning($"[{this}] no scrubbable selected.\nYou can create more scrubbables by extending {nameof(Scrubbable)} class");
                return;
            }

            if (triggerMethod is null) {
                Debug.LogWarning("WAT");
                return;
            }

            var t = Mathf.Clamp01((time - start) / duration);
            scrubbableMethod.Invoke(scrubbable, t);
        }

        private void ExecuteTrigger()
        {
            if (triggerable is null) {
                Debug.LogWarning($"[{this}] no triggerable selected.\nIf no triggerable is available consider adding a {nameof(TriggeredBehaviour)} to the track's target");
                return;
            }

            if (triggerMethod is null) {
                Debug.LogWarning("WAT");
                return;
            }

            triggerMethod.Invoke(triggerable);
        }

        private static void ExecuteSequence()
        {
            throw new NotImplementedException();
            // sequenceMethod.Invoke(sequence);
        }

        public override string ToString()
        {
            return kind switch {
                Kind.Scrubbable => $"Scrubbable: {scrubbable?.GetType().Name}.{scrubbableMethod}",
                Kind.Trigger => $"Trigger: {triggerable?.GetType().Name}.{triggerMethod}",
                // Kind.Sequence => $"Sequence: {scrubbable?.GetType().Name}.{scrubbableMethod}",
                _ => null,
            } ?? "Generic Clip";
        }

        private static IEnumerable<Type> GetScrubbables()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(domainAssembly => domainAssembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Scrubbable)) && !type.IsAbstract)
                .ToArray();
        }
    }
}
