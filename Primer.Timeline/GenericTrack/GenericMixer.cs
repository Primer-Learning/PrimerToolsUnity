using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
                .Where(x => x.start < time)
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

                if (/*behaviour.start <= time && */ behaviour.end >= time)
                    behaviour.Execute(time);
            }
        }


        private readonly List<Triggerable> preparedTriggers = new();
        private readonly List<TriggerablePlayable> ranTriggers = new();
        private void MixTriggerables(TriggerablePlayable[] behaviours, float time, uint iteration)
        {
            var alreadyExecuted = new Queue<TriggerablePlayable>(ranTriggers);
            // var toPrepare = new List<TriggeredBehaviour>();
            var toExecute = new Queue<TriggerablePlayable>();

            for (var i = 0; i < behaviours.Length; i++) {
                if (alreadyExecuted.Count == 0 || behaviours[i] == alreadyExecuted.Dequeue()) {
                    toExecute.Enqueue(behaviours[i]);
                    continue;
                }

                foreach (var ran in ranTriggers)
                    ran.triggerable.Cleanup();

                ranTriggers.Clear();
                toExecute = new Queue<TriggerablePlayable>(behaviours);
                break;
            }

            foreach (var behaviour in toExecute) {
                if (ranTriggers.All(x => x.triggerable != behaviour.triggerable))
                    behaviour.triggerable.Prepare();

                behaviour.Execute(time);
                ranTriggers.Add(behaviour);
            }
        }


        private Dictionary<Sequence, (int, IAsyncEnumerator<object>)> lastSteps = new();
        private async void MixSequentials(SequentialPlayable[] allSequences, float time, uint iteration)
        {
            var steps = new Dictionary<Sequence, (int, IAsyncEnumerator<object>)>();

            foreach (var entry in allSequences.GroupBy(x => x.playableName)) {
                var behaviours = entry.ToArray();
                var sequence = behaviours[0].sequence;

                var (lastStepsCount, lastEnumerator) = lastSteps.ContainsKey(sequence)
                    ? lastSteps[sequence]
                    : (0, null);

                if (behaviours.Length == lastStepsCount) {
                    steps.Add(sequence, (lastStepsCount, lastEnumerator));
                    continue;
                }

                if (behaviours.Length > lastStepsCount) {
                    lastEnumerator ??= behaviours[0].Execute();

                    for (var i = lastStepsCount; i < behaviours.Length; i++) {
                        if (!await lastEnumerator.MoveNextAsync()) {
                            break;
                        }

                        if (currentIteration != iteration)
                            return;
                    }

                    steps[sequence] = (behaviours.Length, lastEnumerator);
                    continue;
                }

                if (lastEnumerator is not null) {
                    await lastEnumerator.DisposeAsync();

                    if (currentIteration != iteration)
                        return;
                }

                var enumerator = behaviours[0].Execute();
                sequence.Prepare();

                for (var i = 0; i < behaviours.Length; i++) {
                    if (!await enumerator.MoveNextAsync()) {
                        break;
                    }

                    if (currentIteration != iteration)
                        return;
                }

                steps[sequence] = (behaviours.Length, enumerator);
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

                if (inputPlayable.GetBehaviour() is {} behaviour)
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
