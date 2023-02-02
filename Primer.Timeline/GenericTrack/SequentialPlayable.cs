using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class SequentialPlayable : GenericBehaviour
    {
        [Space]
        [SerializeReference]
        [ValueDropdown(nameof(GetSequenceOptions))]
        internal Sequence sequence;

        [SerializeField]
        [ShowIf(nameof(sequence))]
        [MethodOf(nameof(sequence), returnType = typeof(IAsyncEnumerator<object>))]
        internal MethodInvocation sequenceMethod;


        public override string icon => "≡";
        public override string playableName => sequenceMethod.ToString(sequence);


        protected override void Start()
        {
            if (sequence != null)
                sequence.Prepare();
        }

        protected override void Stop()
        {
            if (sequence != null)
                sequence.Cleanup();
        }

        public override void Execute(float time)
        {
            // TODO
            throw new NotImplementedException();

            if (sequence is null) {
                Debug.LogWarning(
                    $"[{this}] no sequence selected.\nIf no sequence is available consider adding a {nameof(Sequence)} to the track's target"
                );

                return;
            }

            sequenceMethod.Invoke(sequence);
        }

        public override string ToString()
        {
            return $"Sequence {icon} {sequenceMethod.ToString(sequence)}";
        }

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            if (sequence is null && trackTarget != null)
                sequence = trackTarget.GetComponent<Sequence>();
        }

        private Sequence[] GetSequenceOptions()
        {
            return trackTarget == null
                ? Array.Empty<Sequence>()
                : trackTarget.GetComponents<Sequence>();
        }
    }
}
