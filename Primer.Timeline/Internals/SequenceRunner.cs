using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    public class SequenceRunner : IPrimer, IDisposable
    {
        private readonly IAsyncEnumerator<Tween> enumerator;
        private bool isRunning = false;

        public readonly Transform transform;
        public readonly Sequence sequence;
        public Component component => sequence;

        public bool hasMoreClips { get; private set; } = true;

        public SequenceRunner(Sequence sequence)
        {
            transform = sequence.transform;
            enumerator = sequence.Define();
            this.sequence = sequence;
            sequence.Cleanup();
        }

        public async UniTask<Tween> NextClip()
        {
            if (isRunning)
                throw new Exception("SequenceRunner is already running");

            if (!hasMoreClips)
                throw new Exception("SequenceRunner has no more clips");

            isRunning = true;
            hasMoreClips = await enumerator.MoveNextAsync();
            isRunning = false;
            return enumerator.Current;
        }

        public void Dispose()
        {
            enumerator.DisposeAsync();
        }
    }
}
