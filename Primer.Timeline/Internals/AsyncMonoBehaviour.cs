using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer.Timeline
{
    public class AsyncMonoBehaviour : MonoBehaviour
    {
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
