using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Primer.Timeline
{
    internal class SequenceMixer
    {
        public SingleExecutionGuarantee executionGuarantee = new();
        public readonly Dictionary<Sequence, SequencePlayer> players = new();

        public void Reset()
        {
            players.Clear();
        }

        public void Mix(SequencePlayable[] allBehaviours, float time)
        {
            var ct = executionGuarantee.NewExecution();

            var bySequence = allBehaviours
                .Where(x => x.sequence is not null)
                .GroupBy(x => x.sequence)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var (sequence, behaviours) in bySequence) {
                GetPlayerFor(sequence)
                    .PlayTo(time, behaviours, ct)
                    .Forget();
            }
        }

        private SequencePlayer GetPlayerFor(Sequence sequence)
        {
            if (players.TryGetValue(sequence, out var player))
                return player;

            player = new SequencePlayer(sequence);
            players.Add(sequence, player);
            return player;
        }
    }
}
