using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    [DisallowMultipleComponent]
    public abstract class Sequence : AsyncMonoBehaviour
    {
        private readonly List<Transform> disposeOnCleanup = new();

        public virtual void Cleanup()
        {
            disposeOnCleanup.Dispose();
            disposeOnCleanup.Clear();
        }

        protected void DisposeOnCleanup(Component child)
        {
            disposeOnCleanup.Add(child.transform);
            SequenceOrchestrator.EnsureDisposal(child);
        }

        public SequenceRunner Run() => new(this);

        // TODO: Make protected
        public abstract IAsyncEnumerator<Tween> Define();
    }
}
