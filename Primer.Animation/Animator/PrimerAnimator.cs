using System;
using UnityEngine;

namespace Primer.Animation
{
    [Serializable]
    public abstract class PrimerAnimator
    {
        public AnimationCurve curve = IPrimerAnimation.cubic;

        public abstract void Evaluate(Transform target, float t);
    }
}
