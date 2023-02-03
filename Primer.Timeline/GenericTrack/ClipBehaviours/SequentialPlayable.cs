using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    internal class SequentialPlayable : GenericBehaviour
    {
        public enum StepExecutionResult { Continue, Abort, Done }
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


        #region Clip name
        static SequentialPlayable() => SetIcon<SequentialPlayable>('≡');

        public override string playableName
            => sequence == null ? NO_SEQUENCE_SELECTED : sequenceMethod.ToString(sequence);
        #endregion


        #region Sequence management
        private static HashSet<Sequence> cleannessTracker = new();

        public void Prepare() => Prepare(sequence);
        public static void Prepare(Sequence sequence)
        {
            if (sequence == null || !cleannessTracker.Contains(sequence))
                return;

            sequence.Prepare();
            cleannessTracker.Remove(sequence);
        }

        public void Cleanup() => Cleanup(sequence);
        public static void Cleanup(Sequence sequence)
        {
            if (sequence == null || cleannessTracker.Contains(sequence))
                return;

            sequence.Cleanup();
            cleannessTracker.Add(sequence);
        }

        public IAsyncEnumerator<object> Initialize()
        {
            Prepare();
            return (IAsyncEnumerator<object>)sequenceMethod.Invoke(sequence);
        }

        public async UniTask<IAsyncEnumerator<object>> Execute(int count, Func<bool> shouldAbort)
        {
            var enumerator = Initialize();
            var result = await RunSteps(enumerator, count, shouldAbort);

            if (result == StepExecutionResult.Continue)
                return enumerator;

            await enumerator.DisposeAsync();
            return null;
        }

        public async UniTask<StepExecutionResult> RunSteps(IAsyncEnumerator<object> enumerator, int count, Func<bool> shouldAbort)
        {
            for (var i = 0; i < count; i++) {
                if (!await enumerator.MoveNextAsync())
                    return StepExecutionResult.Done;

                if (shouldAbort())
                    return StepExecutionResult.Abort;
            }

            return StepExecutionResult.Continue;
        }
        #endregion


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
