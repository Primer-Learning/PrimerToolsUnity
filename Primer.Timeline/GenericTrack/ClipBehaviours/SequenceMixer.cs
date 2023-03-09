using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Timeline
{
    internal class SequenceMixer
    {
        private record SequenceState(Sequence sequence, int steps = 0, IAsyncEnumerator<object> enumerator = null);

        public uint currentIteration;

        private Dictionary<PropertyName, SequenceState> previouslyRan = new();
        private readonly HashSet<SequencePlayable> ranBehaviours = new();

        public async void Mix(SequencePlayable[] allBehaviours, float time, uint iteration)
        {
            // In play mode clips actually take time to execute so we need a different approach
            if (Application.isPlaying)
                await PlayModeMix(allBehaviours, time);
            else
                await EditModeMix(allBehaviours, time, iteration);
        }

        private async UniTask PlayModeMix(SequencePlayable[] allBehaviours, float time)
        {
            if (time == 0) {
                foreach (var behaviour in allBehaviours)
                    behaviour.Cleanup();
            }

            var clips = allBehaviours.Where(x => x.weight > 0);

            foreach (var behaviour in clips) {
                if (ranBehaviours.Contains(behaviour))
                    continue;

                ranBehaviours.Add(behaviour);

                var method = behaviour.playableName;
                var last = previouslyRan.Get(method) ?? new SequenceState(behaviour.sequence);
                var lastEnumerator = last.enumerator ?? behaviour.Initialize();

                var result = await behaviour.RunOneStep(lastEnumerator);
                var isOver = result != SequencePlayable.StepExecutionResult.Continue;

                if (isOver)
                    await lastEnumerator.DisposeAsync();

                previouslyRan[method] = last with {
                    steps = last.steps + 1,
                    enumerator = isOver ? null : lastEnumerator,
                };
            }
        }

        private async UniTask EditModeMix(IEnumerable<SequencePlayable> allBehaviours, float time, uint iteration)
        {
            var currentlyRunning = new Dictionary<PropertyName, SequenceState>();

            #region Utility functions
            // If this returns true it means that the mixer has been called again and we should stop
            bool IsExecutionObsolete() => currentIteration != iteration;

            async UniTask<bool> DisposeEnumerator(IAsyncEnumerator<object> enumeratorToDispose)
            {
                await enumeratorToDispose.DisposeAsync();
                return IsExecutionObsolete();
            }
            #endregion

            foreach (var entry in allBehaviours.GroupBy(x => x.playableName)) {
                if (entry.Key == SequencePlayable.NO_SEQUENCE_SELECTED)
                    continue;

                var method = new PropertyName(entry.Key);
                var behaviours = entry.Where(x => x.start <= time).ToArray();
                var stepsToRun = behaviours.Length;

                if (stepsToRun == 0) {
                    SequencePlayable.Cleanup(entry.First().sequence);
                    continue;
                }

                // All behaviours point to the same method of the same sequence, we pick the first one to represent them all
                var behaviour = behaviours[0];
                var sequence = behaviour.sequence;
                var last = previouslyRan.Get(method) ?? new SequenceState(sequence);

                // The executed steps match the amount of steps in the track so nothing to do here
                if (stepsToRun == last.steps) {
                    currentlyRunning.Add(method, last);
                    continue;
                }

                // There were some executions but now we have more steps to execute
                if (stepsToRun > last.steps) {
                    // TODO: Doesn't Initialize trigger the first ran?
                    var lastEnumerator = last.enumerator ?? behaviour.Initialize();

                    var remaining = stepsToRun - last.steps;
                    var result = await behaviour.RunSteps(lastEnumerator, remaining, IsExecutionObsolete);
                    var isOver = result != SequencePlayable.StepExecutionResult.Continue;

                    if (isOver && await DisposeEnumerator(lastEnumerator)) {
                        return;
                    }

                    currentlyRunning[method] = new SequenceState(sequence, stepsToRun, isOver ? null : lastEnumerator);
                    continue;
                }

                // At this point we have less steps to execute than before so we need to re-execute the sequence from the start
                if (last.enumerator is not null && await DisposeEnumerator(last.enumerator))
                    return;

                behaviour.Cleanup();

                var enumerator = await behaviour.Execute(stepsToRun, IsExecutionObsolete);
                currentlyRunning[method] = new SequenceState(sequence, stepsToRun, enumerator);
            }

            // Cleanup the sequences that are no longer in the track
            foreach (var (method, execution) in previouslyRan) {
                if (currentlyRunning.ContainsKey(method))
                    continue;

                SequencePlayable.Cleanup(execution.sequence);

                if (execution.enumerator is not null && await DisposeEnumerator(execution.enumerator)) {
                    return;
                }
            }

            previouslyRan = currentlyRunning;
        }
    }
}
