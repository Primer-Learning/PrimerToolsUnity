using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    // For this track mixer in particular we skip the PrimerBehaviour and just use PlayableBehaviour
    //  because this is a special case where we want to execute past clips and explore future ones.
    internal class SequenceMixer : PrimerMixer
    {
        private Playable lastPlayable;
        public SingleExecutionGuarantee executionGuarantee = new();
        public static readonly Dictionary<Sequence, SequencePlayer> players = new();


        public override void OnPlayableDestroy(Playable playable)
        {
            foreach (var player in players.Values)
                player.Clean();

            players.Clear();
            base.OnPlayableDestroy(playable);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            lastPlayable = playable;

            var mixers = GetAllSequenceMixers(playable.GetGraph());
            var isLast = mixers.Last() == this;
            if (!isLast) return;

            var time = (float)playable.GetTime();

            var allBehaviours = mixers
                .SelectMany(mixer => CollectBehaviours(mixer.lastPlayable))
                .ToArray();

            PlayBehaviours(allBehaviours, time);

            if (allBehaviours.Count(x => x.start <= time) == 0)
                PrimerTimeline.DisposeEphemeralObjects();
        }


        private void PlayBehaviours(IEnumerable<SequencePlayable> allBehaviours, float time)
        {
            var ct = executionGuarantee.NewExecution();

            var bySequence = allBehaviours
                .Where(x => x.trackTarget is not null)
                .GroupBy(x => x.trackTarget)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var (sequence, behaviours) in bySequence) {
                GetPlayerFor(sequence)
                    .PlayTo(time, behaviours, ct)
                    .Forget();
            }
        }

        public static SequencePlayer GetPlayerFor(Sequence sequence)
        {
            if (players.TryGetValue(sequence, out var player))
                return player;

            player = new SequencePlayer(sequence);
            players.Add(sequence, player);
            return player;
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
