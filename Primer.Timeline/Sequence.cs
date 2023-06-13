using System.Collections.Generic;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    [DisallowMultipleComponent]
    public abstract class Sequence : AsyncMonoBehaviour
    {
        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract IAsyncEnumerator<Tween> Define();
    }
}
