using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    internal class SequencePlayer
    {
        private enum Status { Idle, Cleaned, Playing, Done }
        private Status status = Status.Idle;

        private readonly Sequence sequence;
        private readonly List<SequencePlayable> ran = new();
        private IAsyncEnumerator<Tween> enumerator;
        private Tween currentTween;
        private bool isDone = false;

        public SequencePlayer(Sequence sequence)
        {
            this.sequence = sequence;
        }

        public void Clean()
        {
            if (status == Status.Cleaned)
                return;

            status = Status.Cleaned;
            sequence.Cleanup();
        }

        public void Prepare()
        {
            if (status == Status.Playing)
                return;

            status = Status.Playing;
            sequence.Prepare();
        }

        private async UniTask Reset()
        {
            ran.Clear();
            status = Status.Idle;
            isDone = false;

            if (enumerator is null)
                return;

            await enumerator.DisposeAsync();
            enumerator = null;
        }


        public async UniTask PlayTo(float time, List<SequencePlayable> clips, CancellationToken ct)
        {
            #region Restart if necessary
            var needsRestart = ran.Where((ranClip, i) => ranClip != clips[i]).Any();

            if (needsRestart) {
                await Reset();

                if (ct.IsCancellationRequested)
                    return;

                Clean();
                Prepare();
            }
            #endregion

            foreach (var pastClipToRun in clips.Where(x => x.end <= time && !ran.Contains(x))) {
                if (await MoveNext(ct))
                    return;

                currentTween?.Evaluate(1);
                ran.Add(pastClipToRun);
            }

            #region Run current clip
            var currentClips = clips.Where(x => x.start <= time && x.end > time).ToArray();

            if (currentClips.Length == 0)
                return;

            if (currentClips.Length > 1) {
                Debug.LogWarning($"Multiple clips are running for the same sequence {sequence}. This is not supported.");
            }

            var current = currentClips[0];

            if (!ran.Contains(current)) {
                if (await MoveNext(ct))
                    return;

                ran.Add(current);
            }

            if (currentTween is null)
                return;

            var progress = (time - current.start) / current.duration;
            currentTween.Evaluate(progress);
            #endregion
        }

        private async UniTask<bool> MoveNext(CancellationToken ct)
        {
            if (isDone) {
                Debug.LogWarning($"Sequence {sequence} has more clips that yield returns.");
                return false;
            }

            enumerator ??= sequence.Run();
            var hasMore = await enumerator.MoveNextAsync();

            if (ct.IsCancellationRequested)
                return true;

            currentTween = enumerator.Current;

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
    }
}
