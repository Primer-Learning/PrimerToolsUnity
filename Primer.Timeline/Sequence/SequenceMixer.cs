using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    /*
     *  Receives a call to `ProcessFrame(Playable playable)`
     *  for each clip in the track
     *
     *  This is where we convert all those calls
     *  ProcessFrame(Playable playable)
     *  into a single call to
     *  SequenceOrchestrator.PlayAt(SequencePlayable[] allBehaviours, float time)
     *
     *  The way we do that is not nice
     *  but we need it to process the sequences in the right order
     */

    // For this track mixer in particular we skip the PrimerBehaviour and just use PlayableBehaviour
    //  because this is a special case where we want to execute past clips and explore future ones.
    internal class SequenceMixer : PrimerMixer
    {
        private static int instances = 0;
        private Playable lastPlayable;

        public SequenceMixer()
        {
            if (instances is 0)
                Debug.Log("SequenceMixer initiated");

            instances++;
        }

        ~SequenceMixer()
        {
            instances--;

            if (instances is 0) {
                SequenceOrchestrator.Clear();
                Debug.Log("SequenceMixer cleared");
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            lastPlayable = playable;

            var mixers = GetAllSequenceMixers(playable.GetGraph());
            var isLast = mixers.Last() == this;
            if (!isLast) return;

            var allBehaviours = mixers
                .SelectMany(mixer => CollectBehaviours(mixer.lastPlayable))
                .ToArray();

            SequenceOrchestrator.PlayTo(allBehaviours, (float)playable.GetTime());
        }

        private static IEnumerable<SequencePlayable> CollectBehaviours(Playable playable)
        {
            var behaviours = new List<SequencePlayable>();

            for (var i = 0; i < playable.GetInputCount(); i++) {
                var inputPlayable = (ScriptPlayable<SequencePlayable>)playable.GetInput(i);

                if (inputPlayable.GetBehaviour() is not {} behaviour)
                    continue;

                behaviour.weight = playable.GetInputWeight(i);
                behaviours.Add(behaviour);
            }

            behaviours.Sort(new PlayableTimeComparer());
            return behaviours;
        }

        private List<SequenceMixer> GetAllSequenceMixers(PlayableGraph graph)
        {
            var mixers = new List<SequenceMixer>();

            for (var rootIndex = 0; rootIndex < graph.GetRootPlayableCount(); rootIndex++) {
                var root = graph.GetRootPlayable(rootIndex);

                for (var inputIndex = 0; inputIndex < root.GetInputCount(); inputIndex++) {
                    try {
                        var playable = (ScriptPlayable<SequenceMixer>)root.GetInput(inputIndex);
                        var behaviour = playable.GetBehaviour();

                        if (behaviour is not null && behaviour.isMuted == false)
                            mixers.Add(behaviour);
                    }
                    // TODO: prevent error throw when the Playable can't be casted to ScriptPlayable<SequenceMixer2>
                    catch {}
                }
            }

            return  mixers;
        }
    }
}
