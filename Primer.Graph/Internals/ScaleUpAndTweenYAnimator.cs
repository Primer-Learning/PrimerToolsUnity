using System;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class ScaleUpAndTweenYAnimator : PrimerAnimator
    {
        static readonly PrimerAnimator scale = new ScaleUpFromZeroAnimator();
        static readonly PrimerAnimator tween = new TweenYAnimator();

        public float threshold = 0.5f;

        public override void Evaluate(Transform target, float t) {
            var curved = curve.Evaluate(t);
            var scaleT = curved.Remap(0, threshold, 0, 1);
            var tweenT = curved.Remap(threshold, 1, 0, 1);

            scale.Evaluate(target, scaleT);
            tween.Evaluate(target, tweenT);
        }
    }
}
