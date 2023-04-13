using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [ExecuteAlways]
    public abstract class Triggerable : AsyncMonoBehaviour
    {
        public virtual void Prepare() {}
        public virtual void Cleanup() {}
    }
}
