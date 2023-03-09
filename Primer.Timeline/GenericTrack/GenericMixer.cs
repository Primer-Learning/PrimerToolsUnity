using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    // For this track mixer in particular we skip the PrimerBehaviour and just use PlayableBehaviour
    //  because this is a special case where we want to execute past clips and explore future ones.
    public class GenericMixer : PlayableBehaviour
    {
        private uint currentIteration = 0;
        private Playable lastPlayable;

        private readonly ScrubbableMixer scrubbableMixer = new();
        private readonly TriggerableMixer triggerableMixer = new();
        private readonly SequenceMixer sequenceMixer = new();

        public bool isMuted;


        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            lastPlayable = playable;

            var mixers = GetAllGenericMixers(playable.GetGraph());
            var isLast = mixers.Last() == this;

            if (!isLast)
                return;

            var iteration = ++currentIteration;
            var time = (float)playable.GetTime();

            var behaviours = mixers
                .SelectMany(mixer => CollectBehaviours(mixer.lastPlayable))
                .GroupBy(x => x.GetType())
                .ToDictionary(x => x.Key, x => x.ToList());

            // We tell the sequential mixer instance what the current iteration is so it can abort previous executions
            sequenceMixer.currentIteration = currentIteration;

            RunStrategy<ScrubbablePlayable>(scrubbableMixer.Mix, behaviours, time, iteration);
            RunStrategy<TriggerablePlayable>(triggerableMixer.Mix, behaviours, time, iteration);
            RunStrategy<SequencePlayable>(sequenceMixer.Mix, behaviours, time, iteration);
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

        private List<GenericMixer> GetAllGenericMixers(PlayableGraph graph)
        {
            var mixers = new List<GenericMixer>();

            for (var rootIndex = 0; rootIndex < graph.GetRootPlayableCount(); rootIndex++) {
                var root = graph.GetRootPlayable(rootIndex);

                for (var inputIndex = 0; inputIndex < root.GetInputCount(); inputIndex++) {
                    try
                    {
                        var playable = (ScriptPlayable<GenericMixer>)root.GetInput(inputIndex);
                        var behaviour = playable.GetBehaviour();

                        if (behaviour is not null && behaviour.isMuted == false)
                            mixers.Add(behaviour);
                    }
                    // TODO: prevent error throw when the Playable can't be casted to ScriptPlayable<GenericMixer>
                    catch {}
                }
            }

            return  mixers;
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
