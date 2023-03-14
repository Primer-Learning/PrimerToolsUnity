using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    /// <summary>
    ///   Handles the execution of a sequence.
    ///   Gets a list of clips and the time and executes the clips that are before the time.
    ///   It will also execute tween the clip that is currently running.
    /// </summary>
    internal class SequencePlayer
    {
        private enum Status { Idle, Cleaned, Playing, Done }
        private Status status = Status.Idle;

        private readonly Sequence sequence;
        private readonly List<SequencePlayable> completedClips = new();
        private SequencePlayable currentClip;
        private IAsyncEnumerator<Tween> enumerator;
        private Tween currentTween;
        private bool isDone = false;
        private int index = 0;

        public SequencePlayer(Sequence sequence)
        {
            this.sequence = sequence;
        }

        /// <summary>Sets the state before any clip is executed</summary>
        public void Clean()
        {
            if (status == Status.Cleaned)
                return;

            Log("Clean");
            status = Status.Cleaned;
            sequence.Cleanup();
        }

        /// <summary>Executed immediately before any clip is executed</summary>
        public void Prepare()
        {
            if (status == Status.Playing)
                return;

            Log("Prepare");
            status = Status.Playing;
            sequence.Prepare();
        }

        /// <summary>Rolls back the sequence execution to the beginning</summary>
        private async UniTask Reset()
        {
            Log("Restart");
            completedClips.Clear();
            currentClip = null;
            isDone = false;
            index = 0;

            if (enumerator is null)
                return;

            await enumerator.DisposeAsync();
            enumerator = null;
        }


        public async UniTask PlayTo(float time, List<SequencePlayable> clips, CancellationToken ct)
        {
            var clipsToRun = clips.Where(x => x.start <= time).ToArray();

            if (await RestartIfNecessary(clipsToRun, ct))
                return;

            var pastClips = clipsToRun.Where(x => x.end <= time);

            if (await ExecutePastClips(pastClips, ct))
                return;

            var currentClips = clipsToRun.Where(x => x.end > time).ToArray();

            if (await ExecuteCurrentClip(time, currentClips, ct))
                // yes, there is no need for this return but shows that all three operations follow the same pattern
                return;
        }

        private async Task<bool> ExecutePastClips(IEnumerable<SequencePlayable> pastClips, CancellationToken ct)
        {
            foreach (var pastClipToRun in pastClips.Where(x => !completedClips.Contains(x))) {
                if (await MoveNext(pastClipToRun, ct))
                    return true;

                currentTween?.Evaluate(1);
                completedClips.Add(pastClipToRun);
            }

            return false;
        }

        private async Task<bool> RestartIfNecessary(SequencePlayable[] clipsToRun, CancellationToken ct)
        {
            if (clipsToRun.Length == 0) {
                if (status == Status.Cleaned)
                    return true;

                await Reset();

                if (ct.IsCancellationRequested)
                    return true;

                Clean();
                return true;
            }

            var areRanClipsValid =
                completedClips.Count <= clipsToRun.Length &&
                completedClips.Where((ranClip, i) => ranClip == clipsToRun[i]).Any();

            if (areRanClipsValid)
                return false;

            await Reset();

            if (ct.IsCancellationRequested)
                return true;

            Clean();
            Prepare();

            return false;
        }

        private async Task<bool> ExecuteCurrentClip(float time, SequencePlayable[] currentClips, CancellationToken ct)
        {
            if (currentClips.Length == 0)
                return true;

            if (currentClips.Length > 1) {
                Debug.LogWarning($"Multiple clips are running for the same sequence {sequence}. This is not supported.");
            }

            var current = currentClips[0];

            if (currentClip != current && await MoveNext(current, ct))
                return true;

            if (currentTween is null)
                return true;

            var progress = (time - current.start) / current.duration;
            Log($"Execute tween at time {time} - {progress}");
            currentTween.Evaluate(progress);
            return false;
        }

        private async UniTask<bool> MoveNext(SequencePlayable clip, CancellationToken ct)
        {
            Log($"Executing clip {++index}");

            if (isDone) {
                Debug.LogWarning($"Sequence {sequence} has more clips that yield returns.");
                return false;
            }

            enumerator ??= sequence.Run();
            var hasMore = await enumerator.MoveNextAsync();

            if (ct.IsCancellationRequested)
                return true;

            currentClip = clip;
            currentTween = enumerator.Current;

            if (currentTween is not null)
                clip.ReportDuration(currentTween.totalDuration);

            if (hasMore)
                return false;

            isDone = true;
            await enumerator.DisposeAsync();

            if (ct.IsCancellationRequested)
                return true;

            enumerator = null;
            status = Status.Done;
            return false;
        }

        public static void Log(params object[] args)
        {
            // PrimerLogger.Log("SequencePlayer", args);
        }
    }
}
