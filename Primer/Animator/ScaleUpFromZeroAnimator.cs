using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Primer
{
    [Serializable]
    [UsedImplicitly]
    public class ScaleUpFromZeroAnimator : PrimerAnimator
    {
        public override void Evaluate(Transform target, float t) {
            target.localScale = target.localScale * curve.Evaluate(t);
        }
    }
}
