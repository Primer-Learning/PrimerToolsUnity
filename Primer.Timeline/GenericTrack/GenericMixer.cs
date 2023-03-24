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
        private Playable lastPlayable;

        private readonly ScrubbableMixer scrubbableMixer = new();
        private readonly TriggerableMixer triggerableMixer = new();
        private readonly SequenceMixer sequenceMixer = new();

        public bool isMuted;


        public override void OnPlayableDestroy(Playable playable)
        {
            PrimerTimeline.DisposeEphemeralObjects();
            sequenceMixer.Reset();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            lastPlayable = playable;

            var mixers = GetAllGenericMixers(playable.GetGraph());
            var isLast = mixers.Last() == this;

            if (!isLast)
                return;

            var time = (float)playable.GetTime();

            var allBehaviours = mixers
                .SelectMany(mixer => CollectBehaviours(mixer.lastPlayable))
                .ToArray();

            var behaviours = allBehaviours
                .GroupBy(x => x.GetType())
                .ToDictionary(x => x.Key, x => x.ToList());

            RunStrategy<ScrubbablePlayable>(scrubbableMixer.Mix, behaviours, time);
            RunStrategy<TriggerablePlayable>(triggerableMixer.Mix, behaviours, time);
            RunStrategy<SequencePlayable>(sequenceMixer.Mix, behaviours, time);

            if (allBehaviours.Count(x => x.start < time) == 0)
                PrimerTimeline.DisposeEphemeralObjects();
        }


        private static void RunStrategy<T>(Action<T[], float> strategy,
            IReadOnlyDictionary<Type, List<GenericBehaviour>> dictionary,
            float time)
            where T : GenericBehaviour
        {
            if (dictionary.ContainsKey(typeof(T))) {
                strategy(dictionary[typeof(T)].Cast<T>().ToArray(), time);
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
}
