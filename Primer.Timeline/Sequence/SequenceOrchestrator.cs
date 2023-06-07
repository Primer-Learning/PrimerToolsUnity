using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    internal static class SequenceOrchestrator
    {
        private static readonly SingleExecutionGuarantee executionGuarantee = new();
        private static readonly Dictionary<Sequence, SequencePlayer> players = new();
        private static readonly List<UniTask> tasks = new();

        public static UniTask AllSequencesFinished() => UniTask.WhenAll(tasks);

        public static void PlayTo(SequencePlayable[] behaviours, float time)
        {
            PlayBehaviours(behaviours, time);

            if (behaviours.Count(x => x.start <= time) == 0)
                PrimerTimeline.DisposeEphemeralObjects();
        }

        private static void PlayBehaviours(IEnumerable<SequencePlayable> allBehaviours, float time)
        {
            var ct = executionGuarantee.NewExecution();

            tasks.Clear();

            var bySequence = allBehaviours
                .Where(x => x.trackTarget is not null)
                .GroupBy(x => x.trackTarget)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var (sequence, behaviours) in bySequence) {
                var task = GetPlayerFor(sequence).PlayTo(time, behaviours, ct);
                tasks.Add(task);
            }
        }

        public static void Clear()
        {
            foreach (var player in players.Values) {
                player.Reset().Forget();
                player.Clean();
            }

            players.Clear();
            tasks.Clear();
        }

        internal static SequencePlayer GetPlayerFor(Sequence sequence)
        {
            if (players.TryGetValue(sequence, out var player)) {
                if (player.isInvalid)
                    players.Remove(sequence);
                else
                    return player;
            }

            player = new SequencePlayer(sequence);
            players.Add(sequence, player);
            return player;
        }
    }
}
