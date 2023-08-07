using System;
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
    ///     Handles the execution of a sequence.
    ///     Gets a list of clips and the time and executes the clips that are before the time.
    ///     It will also execute tween the clip that is currently running.
    /// </summary>
    internal class SequencePlayer
    {
        private enum Status { Idle, Cleaned, Playing, Done }
        private Status status = Status.Idle;

        private readonly Sequence sequence;
        private readonly List<SequencePlayable> completedClips = new();
        private IAsyncEnumerator<Tween> enumerator;
        private SequencePlayable activeClip;
        private Tween currentTween;
        private bool isDone = false;

        public bool isInvalid => sequence == null;

        public SequencePlayer(Sequence sequence)
        {
            this.sequence = sequence;
        }

        #region Clean() / Prepare() / Reset()
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
        // TODO: Remove
        [Obsolete("Code that used to be in Prepare() can be added at the beginning of Define(), before the first yield return, instead.")]
        public void Prepare()
        {
            if (status == Status.Playing)
                return;

            Log("Prepare");
            status = Status.Playing;
            sequence.Prepare();
        }

        /// <summary>Rolls back the sequence execution to the beginning</summary>
        public async UniTask Reset()
        {
            Log("Reset");
            completedClips.Clear();
            activeClip = null;
            isDone = false;

            if (enumerator is null)
                return;

            await enumerator.DisposeAsync();
            enumerator = null;
        }
        #endregion

        public async UniTask PlayTo(float time, List<SequencePlayable> clips, CancellationToken ct)
        {
            var clipsToRun = clips.Where(x => x.start <= time).ToArray();
            var pastClips = clipsToRun.Where(x => x.end <= time).ToArray();
            var currentClip = GetCurrentClip(clipsToRun.Where(x => x.end > time).ToArray());

            // Any function returning true means that the execution should be stopped
            // Either because the CancellationToken was cancelled or because there is nothing to do

            if (await RestartIfNecessary(pastClips, currentClip, ct))
                return;

            if (await ExecutePastClips(pastClips, ct))
                return;

            if (await ExecuteCurrentClip(time, currentClip, ct))
                // there is no need for this return but shows that all three operations follow the same pattern
                return;
        }

        private SequencePlayable GetCurrentClip(SequencePlayable[] currentClips)
        {
            if (currentClips.Length == 0)
                return null;

            if (currentClips.Length > 1) {
                Debug.LogWarning($"Multiple clips are running for the same sequence {sequence}. This is not supported.");
            }

            return currentClips[0];
        }

        private async Task<bool> RestartIfNecessary(SequencePlayable[] pastClips, SequencePlayable currentClip, CancellationToken ct)
        {
            if (pastClips.Length == 0 && currentClip is null) {
                if (status == Status.Cleaned)
                    return true;

                await Reset();

                if (ct.IsCancellationRequested)
                    return true;

                Clean();
                return true;
            }

            var areRanClipsValid =
                completedClips.Count <= pastClips.Length &&
                completedClips.Where((ranClip, i) => ranClip == pastClips[i]).Count() == completedClips.Count;

            var isActiveClipValid = activeClip == currentClip || pastClips.Contains(activeClip);

            if (areRanClipsValid && isActiveClipValid)
                return false;

            await Reset();

            if (ct.IsCancellationRequested)
                return true;

            Clean();
            Prepare();

            return false;
        }

        private async Task<bool> ExecutePastClips(SequencePlayable[] pastClips, CancellationToken ct)
        {
            foreach (var pastClipToRun in pastClips.Where(x => !completedClips.Contains(x))) {
                var isActiveClip = activeClip == pastClipToRun;

                if (!isActiveClip && await MoveNext(pastClipToRun, ct))
                    return true;

                currentTween?.Evaluate(1);
                completedClips.Add(pastClipToRun);
            }

            return false;
        }

        private async Task<bool> ExecuteCurrentClip(float time, SequencePlayable currentClip, CancellationToken ct)
        {
            if (currentClip is null)
                return true;

            if (activeClip != currentClip && await MoveNext(currentClip, ct))
                return true;

            if (currentTween is null)
                return true;

            var progress = (time - currentClip.start) / currentClip.duration;
            // Log($"Execute tween at time {time} - {progress}");
            currentTween.Evaluate(progress);
            return false;
        }

        private async UniTask<bool> MoveNext(SequencePlayable clip, CancellationToken ct)
        {
            Log($"Executing clip", clip);

            if (isDone) {
                currentTween = null;
                Debug.LogWarning($"Sequence {sequence} has more clips that yield returns.");
                return false;
            }

            Prepare();

            enumerator ??= sequence.Define();
            var hasMore = await enumerator.MoveNextAsync();

            if (ct.IsCancellationRequested)
                return true;

            if (currentTween is not null)
                currentTween.Dispose();

            activeClip = clip;
            currentTween = hasMore ? enumerator.Current : null;

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



        public void Log(string message, SequencePlayable clip = null)
        {
            // var label = $"{sequence} [Player]";
            //
            // if (clip is null)
            //     PrimerLogger.Log(label, message);
            // else
            //     PrimerLogger.Log(label, message, _clips.IndexOf(clip));
        }
    }
}
