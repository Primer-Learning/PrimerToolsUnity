using System;
using UnityEngine;

namespace Primer.Animation
{
    [Serializable]
    public abstract class PrimerAnimator
    {
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        public abstract void Evaluate(Transform target, float t);
    }
}
