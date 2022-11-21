using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Primer
{
    [Serializable]
    [UsedImplicitly]
    public class TweenYAnimator : PrimerAnimator
    {
        public override void Evaluate(Transform target, float t) {
            var pos = target.localPosition;
            var curved = curve.Evaluate(t);
            target.localPosition = new Vector3(pos.x, pos.y * curved, pos.z);
        }
    }
}
