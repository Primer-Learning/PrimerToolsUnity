using System;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public class GenericClip : PlayableAsset, ITimelineClipAsset
    {
        public enum Kind { Scrubbable, Trigger, Sequence }

        public ClipCaps clipCaps => ClipCaps.Extrapolation;

        [Space(32)]
        [HideLabel]
        [EnumToggleButtons]
        public Kind kind = Kind.Scrubbable;

        [Space]
        [SerializeReference]
        [ShowIf("@kind == Kind.Scrubbable")]
        [DisableContextMenu]
        [HideReferenceObjectPicker]
        [HideLabel]
        internal ScrubbablePlayable scrubbable = new();

        [Space]
        [SerializeReference]
        [ShowIf("@kind == Kind.Trigger")]
        [DisableContextMenu]
        [HideReferenceObjectPicker]
        [HideLabel]
        internal TriggerablePlayable triggerable = new();

        [FormerlySerializedAs("sequential")]
        [Space]
        [SerializeReference]
        [ShowIf("@kind == Kind.Sequence")]
        [DisableContextMenu]
        [HideReferenceObjectPicker]
        [HideLabel]
        internal SequencePlayable sequence = new();

        public GenericBehaviour template => kind switch {
            Kind.Scrubbable => scrubbable,
            Kind.Trigger => triggerable,
            Kind.Sequence => sequence,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public IExposedPropertyTable resolver {
            get => scrubbable.resolver;
            set {
                EnsurePlayablesExist();
                scrubbable.resolver = value;
                triggerable.resolver = value;
                sequence.resolver = value;
            }
        }

        /// <summary>Clips may be set to null when refactoring</summary>
        private void EnsurePlayablesExist()
        {
            scrubbable ??= new ScrubbablePlayable();
            triggerable ??= new TriggerablePlayable();
            sequence ??= new SequencePlayable();
        }

        public Transform trackTarget {
            get {
                if (scrubbable.trackTarget == null
                    || scrubbable.trackTarget != triggerable.trackTarget
                    || scrubbable.trackTarget != sequence.trackTarget) {
                    return null;
                }

                return scrubbable.trackTarget;
            }
            set {
                scrubbable.trackTarget = value;
                triggerable.trackTarget = value;
                sequence.trackTarget = value;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<GenericBehaviour>.Create(graph, template);
        }

        public void Initialize()
        {
            triggerable._triggerable.exposedName = GUID.Generate().ToString();
            sequence._sequence.exposedName = GUID.Generate().ToString();
        }
    }
}
