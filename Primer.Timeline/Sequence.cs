using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Timeline
{
    public abstract class Sequence : MonoBehaviour
    {
        public virtual void Prepare() {}

        public virtual void Cleanup() {}

        public abstract IAsyncEnumerator<object> Run();

        protected static async UniTask Milliseconds(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }

        protected static async UniTask Seconds(float seconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(Mathf.RoundToInt(seconds * 1000));
        }

        protected static async UniTask Parallel(params UniTask[] processes)
        {
            await UniTask.WhenAll(processes);
        }
    }
}
