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


        private static readonly Dictionary<Transform, (IAsyncEnumerator<object> enumerator, int steps)> executions =
            new();

        public override void Execute(float time)
        {
            // TODO
            throw new NotImplementedException();

            if (!executions.ContainsKey(trackTarget)) {
                executions.Add(trackTarget, (sequence.Run(), 0));
                return;
            }

            var (enumerator, steps) = executions[trackTarget];

            // if (steps < )
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
