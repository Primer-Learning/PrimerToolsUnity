using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class SequentialPlayable : GenericBehaviour
    {
        [SerializeReference]
        [ValueDropdown(nameof(GetSequenceOptions))]
        internal Sequence sequence;

        [SerializeField]
        [ShowIf(nameof(sequence))]
        [MethodOf(nameof(sequence), returnType = typeof(IAsyncEnumerator<object>))]
        internal MethodInvocation sequenceMethod;


        public override char icon => '≡';
        public override string playableName => sequenceMethod.ToString(sequence);


        #region Sequence management
        private static HashSet<Sequence> initializationTracker = new();

        public void Prepare()
        {
            if (sequence == null || initializationTracker.Contains(sequence))
                return;

            sequence.Prepare();
            initializationTracker.Add(sequence);
        }

        public void Cleanup()
        {
            if (sequence == null || !initializationTracker.Contains(sequence))
                return;

            sequence.Cleanup();
            initializationTracker.Remove(sequence);
        }
        public IAsyncEnumerator<object> Execute()
        {
            return (IAsyncEnumerator<object>)sequenceMethod.Invoke(sequence);
        }
        #endregion


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
