using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    // For this track in particular we skip the PrimerBehaviour and just use PlayableBehaviour
    //  because this is a special case where we want to execute past clips too.
    public class GenericMixer : PlayableBehaviour
    {
        private uint currentIteration = 0;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            var iteration = ++currentIteration;
            var time = (float)playable.GetTime();

            var behaviours = CollectBehaviours(playable)
                .GroupBy(x => x.GetType())
                .ToDictionary(x => x.Key, x => x.ToList());

            RunStrategy<ScrubbablePlayable>(MixScrubbables, behaviours, time, iteration);
            RunStrategy<TriggerablePlayable>(MixTriggerables, behaviours, time, iteration);
            RunStrategy<SequentialPlayable>(MixSequentials, behaviours, time, iteration);
        }


        private static void MixScrubbables(ScrubbablePlayable[] behaviours, float time, uint iteration)
        {
            for (var i = 0; i < behaviours.Length; i++) {
                var behaviour = behaviours[i];

                if (behaviour.weight == 0)
                    behaviour.Cleanup();
                else
                    behaviour.Execute(time);
            }
        }



        private readonly HashSet<TriggerablePlayable> ranTriggers = new();
        private void MixTriggerables(TriggerablePlayable[] allBehaviours, float time, uint iteration)
        {
            var behaviours = allBehaviours.Where(x => x.start <= time).ToArray();
            var toClean = allBehaviours.Select(x => x.triggerable).ToHashSet();
            var toExecute = new Queue<TriggerablePlayable>();
            var alreadyExecuted = new Queue<TriggerablePlayable>(ranTriggers);
            var rerunPipeline = false;

            for (var i = 0; i < behaviours.Length; i++) {
                var behaviour = behaviours[i];

                if (alreadyExecuted.Count == 0) {
                    toExecute.Enqueue(behaviour);
                    continue;
                }

                if (behaviour.Equals(alreadyExecuted.Dequeue())) {
                    toClean.Remove(behaviour.triggerable);
                    continue;
                }

                rerunPipeline = true;
                break;
            }

            // We have executions from previous frames that we need to remove, we have to re-run the whole chain
            if (rerunPipeline || alreadyExecuted.Count > 0) {

                foreach (var ran in ranTriggers)
                    ran.Cleanup();

                ranTriggers.Clear();
                toExecute = new Queue<TriggerablePlayable>(behaviours);
            }

            foreach (var behaviour in toExecute) {
                behaviour.Execute(time);
                ranTriggers.Add(behaviour);
                toClean.Remove(behaviour.triggerable);
            }

            foreach (var behaviour in toClean.SelectMany(x => allBehaviours.Where(y => y.triggerable == x))) {
                behaviour.Cleanup();
            }
        }


        private Dictionary<Sequence, (int, IAsyncEnumerator<object>)> lastSteps = new();

        private async void MixSequentials(SequentialPlayable[] allSequences, float time, uint iteration)
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

                if (sequence == null)
                    continue;

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

                var enumerator = await behaviour.Execute(stepsToRun, IsExecutionObsolete);
                steps[sequence] = (behaviours.Length, enumerator);
            }

            // Cleanup the sequences that are no longer in the track
            foreach (var (sequence, (_, lastEnumerator)) in lastSteps) {
                if (steps.ContainsKey(sequence))
                    continue;

                SequentialPlayable.Cleanup(sequence);

                if (lastEnumerator is not null && await DisposeEnumerator(lastEnumerator))
                    return;
            }

            lastSteps = steps;
        }


        private static void RunStrategy<T>(Action<T[], float, uint> strategy,
            IReadOnlyDictionary<Type, List<GenericBehaviour>> dictionary,
            float time,
            uint iteration)
            where T : GenericBehaviour
        {
            if (dictionary.ContainsKey(typeof(T))) {
                strategy(dictionary[typeof(T)].Cast<T>().ToArray(), time, iteration);
            }
        }

        private static IEnumerable<GenericBehaviour> CollectBehaviours(Playable playable)
        {
            var behaviours = new List<GenericBehaviour>();

            for (var i = 0; i < playable.GetInputCount(); i++) {
                var inputPlayable = (ScriptPlayable<GenericBehaviour>)playable.GetInput(i);

                if (inputPlayable.GetBehaviour() is not {} behaviour)
                    continue;

                behaviour.weight = playable.GetInputWeight(i);
                behaviours.Add(behaviour);
            }

            behaviours.Sort(new PlayableTimeComparer());
            return behaviours;
        }
    }

    public class PlayableTimeComparer : IComparer<GenericBehaviour>
    {
        public int Compare(GenericBehaviour left, GenericBehaviour right)
        {
            if (left is null && right is null)
                return 0;

            if (left is null)
                return 1;

            if (right is null)
                return -1;

            var delta = left.start - right.start;
            return (int) (delta * 10000);
        }
    }
}
