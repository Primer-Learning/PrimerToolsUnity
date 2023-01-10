using System;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class CollectorWithDirection<T> : CollectorWithDirection<T, T>
        where T : class, IPlayableBehaviour, new()
    {
        public CollectorWithDirection() : base(x => x) {}
    }

    public class CollectorWithDirection<TPlayableBehaviour, TData> : MixerCollector<TPlayableBehaviour, TData>
        where TPlayableBehaviour : class, IPlayableBehaviour, new()
    {
        private double lastTime;
        private float lastWeight;

        public CollectorWithDirection(Func<TPlayableBehaviour, TData> getData) : base(getData) {}

        public bool isReverse { get; private set; }

        public override void Collect(Playable playable)
        {
            base.Collect(playable);

            var time = playable.GetTime();

            var isDecreasing = lastWeight > weight;
            var isBackwards = lastTime > time;

            lastWeight = weight;
            lastTime = time;

            isReverse = isBackwards ? !isDecreasing : isDecreasing;
        }
    }
}
