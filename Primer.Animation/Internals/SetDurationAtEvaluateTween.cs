using System;

namespace Primer.Animation
{
    public record SetDurationAtEvaluateTween(Action<float> lerp, Func<float> durationFunc) : Tween(lerp)
    {
        private bool durationInitialized;
        public override void Evaluate(float t)
        {
            if (!durationInitialized)
            {
                durationInitialized = true;
                duration = durationFunc();
            }
            base.Evaluate(t);
        }
    }
}