using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public class GenericClip : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.Extrapolation;

        [Space(32)]
        [HideLabel]
        [EnumToggleButtons]
        public Kind kind = Kind.Scrubbable;
        public enum Kind { Scrubbable, Trigger, Sequence }

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
        private SequentialPlayable sequential = new();

        public GenericBehaviour template => kind switch {
            Kind.Scrubbable => scrubbable,
            Kind.Trigger => triggerable,
            Kind.Sequence => sequential,
            _ => throw new ArgumentOutOfRangeException(),
        };

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<GenericBehaviour>.Create(graph, template);
        }
    }
}
