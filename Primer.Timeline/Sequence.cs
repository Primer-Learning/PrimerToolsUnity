using System;
using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    [DisallowMultipleComponent]
    public abstract class Sequence : AsyncMonoBehaviour
    {
        // TODO: Remove
        [Obsolete("Code that used to be in Prepare() can be added at the beginning of Define(), before the first yield return, instead.")]
        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public SequenceRunner Run() => new(this);

        // TODO: Make protected
        public abstract IAsyncEnumerator<Tween> Define();
    }
}
