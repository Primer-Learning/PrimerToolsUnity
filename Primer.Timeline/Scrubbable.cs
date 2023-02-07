using System;
using UnityEngine;

namespace Primer
{
    [Serializable]
    public abstract class Scrubbable
    {
        public Transform target { get; set; }

        public virtual void Prepare() {}

        public virtual void Cleanup() {}
    }
}
