using System;
using Cysharp.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Timeline
{
    // TODO: Remove
    [Serializable]
    [Obsolete("Use Primer.Animation.ObservableTween instead")]
    public abstract class Scrubbable
    {
        public Transform target { get; set; }

        // Abstract methods

        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract void Update(float t);

        // Implemented methods

        public void PlayAndForget() => Play();

        public async UniTask Play()
        {
            await AsTween();
        }

        public Tween AsTween()
        {
            return new ObservableTween(Update) {
                beforePlay = Prepare,
            };
        }
    }
}
