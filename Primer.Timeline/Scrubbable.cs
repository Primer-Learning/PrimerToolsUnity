using System;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    [Serializable]
    public abstract class Scrubbable
    {
        public Transform target { get; set; }

        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract void Update(float t);

        public void PlayAndForget() => Play();

        public UniTask Play()
        {
            return AsTween().Play();
        }

        public Tween AsTween()
        {
            return new ObservableTween(Update) {
                beforePlay = Prepare,
            };
        }
    }
}
