using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class SequencePlayable : GenericBehaviour
    {
        public const string NO_SEQUENCE_SELECTED = "No sequence selected";


        [SerializeField]
        [HideInInspector]
        internal ExposedReference<Sequence> _sequence;

        [ShowInInspector]
        [PropertyOrder(1)]
        [ValueDropdown(nameof(GetSequenceOptions))]
        [Tooltip("Components in the track's target that extend Sequence")]
        public Sequence sequence {
            get => resolver.Get(_sequence);
            set => resolver.Set(_sequence, value);
        }

        [SerializeField]
        [PropertyOrder(2)]
        [ShowIf(nameof(sequence))]
        [MethodOf(nameof(sequence), returnType = typeof(IAsyncEnumerator<object>))]
        internal MethodInvocation sequenceMethod;


        public override Transform trackTarget {
            get => base.trackTarget;
            internal set {
                base.trackTarget = value;

                if (value == null)
                    sequence = null;
                else if (sequence is null || sequence.gameObject != value.gameObject)
                    sequence = value.GetComponent<Sequence>();
            }
        }


        static SequencePlayable() => SetIcon<SequencePlayable>('≡');

        public override string playableName
            => sequence == null ? NO_SEQUENCE_SELECTED : sequenceMethod.ToString(sequence);


        public override string ToString()
        {
            return $"Sequence {icon} {sequenceMethod.ToString(sequence)}";
        }

        private Sequence[] GetSequenceOptions()
        {
            return trackTarget == null
                ? Array.Empty<Sequence>()
                : trackTarget.GetComponents<Sequence>();
        }
    }
}
