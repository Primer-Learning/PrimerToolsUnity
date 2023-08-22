using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Timeline
{
    /*
     *  Whenever we move the time cursor in the editor
     *  we compress all that noise and function invocations Unity do
     *  into a single call to SequenceOrchestrator.PlayAt(clips, float time)
     *
     *  This class creates a SequencePlayer for each sequence
     *  (reuses if already created)
     *  and calls PlayAt on it
     */

    internal static class SequenceOrchestrator
    {
        private static readonly SingleExecutionGuarantee executionGuarantee = new();
        private static readonly Dictionary<Sequence, SequencePlayer> players = new();
        private static readonly List<UniTask> tasks = new();

        public static UniTask AllSequencesFinished() => UniTask.WhenAll(tasks);

        public static void PlayTo(SequencePlayable[] allBehaviours, float time)
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

        public static async void Clear()
        {
            foreach (var player in players.Values) {
                await player.Reset();
                player.Clean();
            }

            players.Clear();
            tasks.Clear();

            Object.FindObjectsOfType<RemoveOnCleanup>().Dispose();
        }

        public static void EnsureDisposal(Component target)
        {
            if (target != null)
                target.GetOrAddComponent<RemoveOnCleanup>();
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

        internal class RemoveOnCleanup : MonoBehaviour {}
    }
}
