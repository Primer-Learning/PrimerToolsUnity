using System;
using UnityEngine;

namespace Primer
{
    [Serializable]
    public class NoAnimator : PrimerAnimator
    {
        public override void Evaluate(Transform target, float t) {}
    }
}
