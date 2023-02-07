using System;
using Primer.Timeline.FakeUnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
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

        [Space]
        [SerializeReference]
        [ShowIf("@kind == Kind.Sequence")]
        [DisableContextMenu]
        [HideReferenceObjectPicker]
        [HideLabel]
        internal SequentialPlayable sequential = new();

        public GenericBehaviour template => kind switch {
            Kind.Scrubbable => scrubbable,
            Kind.Trigger => triggerable,
            Kind.Sequence => sequential,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public IExposedPropertyTable resolver {
            get => scrubbable.resolver;
            set {
                scrubbable.resolver = value;
                triggerable.resolver = value;
                sequential.resolver = value;
            }
        }

        public Transform trackTarget {
            get {
                if (scrubbable.trackTarget == null
                    || scrubbable.trackTarget != triggerable.trackTarget
                    || scrubbable.trackTarget != sequential.trackTarget) {
                    return null;
                }

                return scrubbable.trackTarget;
            }
            set {
                scrubbable.trackTarget = value;
                triggerable.trackTarget = value;
                sequential.trackTarget = value;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<GenericBehaviour>.Create(graph, template);
        }

        public void Initialize()
        {
            triggerable._triggerable.exposedName = GUID.Generate().ToString();
            sequential._sequence.exposedName = GUID.Generate().ToString();
        }
    }
}
