using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    internal class SequentialMixer
    {
        public uint currentIteration;
        private Dictionary<Sequence, (int, IAsyncEnumerator<object>)> lastSteps = new();

        public async void Mix(SequentialPlayable[] allSequences, float time, uint iteration)
        {
            var steps = new Dictionary<Sequence, (int, IAsyncEnumerator<object>)>();

            #region Utility functions
            // If this returns true it means that the mixer has been called again and we should stop
            bool IsExecutionObsolete() => currentIteration != iteration;

            async UniTask<bool> DisposeEnumerator(IAsyncEnumerator<object> enumeratorToDispose)
            {
                await enumeratorToDispose.DisposeAsync();
                return IsExecutionObsolete();
            }
            #endregion

            foreach (var entry in allSequences.GroupBy(x => x.playableName)) {
                if (entry.Key == SequentialPlayable.NO_SEQUENCE_SELECTED)
                    continue;

                var behaviours = entry.Where(x => x.start <= time).ToArray();
                var stepsToRun = behaviours.Length;


                if (stepsToRun == 0) {
                    SequentialPlayable.Cleanup(entry.First().sequence);
                    continue;
                }

                // All behaviours point to the same method of the same sequence, we pick the first one to represent them all
                var behaviour = behaviours[0];
                var sequence = behaviour.sequence;

                var (lastStepsCount, lastEnumerator) = lastSteps.ContainsKey(sequence)
                    ? lastSteps[sequence]
                    : (0, null);

                // The executed steps match the amount of steps in the track so nothing to do here
                if (stepsToRun == lastStepsCount) {
                    steps.Add(sequence, (lastStepsCount, lastEnumerator));
                    continue;
                }

                // There were some executions but now we have more steps to execute
                if (stepsToRun > lastStepsCount) {
                    lastEnumerator ??= behaviour.Initialize();

                    var remaining = stepsToRun - lastStepsCount;
                    var result = await behaviour.RunSteps(lastEnumerator, remaining, IsExecutionObsolete);
                    var isOver = result != SequentialPlayable.StepExecutionResult.Continue;

                    if (isOver && await DisposeEnumerator(lastEnumerator))
                        return;

                    steps[sequence] = (stepsToRun, isOver ? null : lastEnumerator);
                    continue;
                }

                // At this point we have less steps to execute than before so we need to re-execute the sequence from the start
                if (lastEnumerator is not null && await DisposeEnumerator(lastEnumerator))
                    return;

                behaviour.Cleanup();

                var enumerator = await behaviour.Execute(stepsToRun, IsExecutionObsolete);
                steps[sequence] = (behaviours.Length, enumerator);
            }

            // Cleanup the sequences that are no longer in the track
            foreach (var (sequence, (_, lastEnumerator)) in lastSteps) {
                if (steps.ContainsKey(sequence)) {
                    continue;
                }

                SequentialPlayable.Cleanup(sequence);

                if (lastEnumerator is not null && await DisposeEnumerator(lastEnumerator)) {
                    return;
                }
            }

            lastSteps = steps;
        }
    }
}
