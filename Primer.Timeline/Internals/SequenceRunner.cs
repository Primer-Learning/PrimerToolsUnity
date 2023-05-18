using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    public class SequenceRunner : IDisposable
    {
        private readonly IAsyncEnumerator<Tween> enumerator;
        private bool hasMore = true;
        private bool isRunning = false;

        public readonly Transform transform;
        public bool hasMoreClips => hasMore;

        public SequenceRunner(Sequence sequence)
        {
            transform = sequence.transform;
            enumerator = sequence.Run();

            sequence.Cleanup();
        }

        public async UniTask<Tween> NextClip()
        {
            if (isRunning)
                throw new Exception("SequenceRunner is already running");

            if (!hasMoreClips)
                throw new Exception("SequenceRunner has no more clips");

            isRunning = true;
            hasMore = await enumerator.MoveNextAsync();
            isRunning = false;
            return enumerator.Current;
        }

        public void Dispose()
        {
            enumerator.DisposeAsync();
        }
    }
}
