using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    // public class PrepareMethodAttribute : Attribute {}

    [ExecuteAlways]
    public abstract class TriggeredBehaviour : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not TriggerMarker trigger || trigger.method is null)
                return;

            var type = GetType();
            var method = type.GetMethod(trigger.method);

            if (method is null)
                throw new Exception($"No method {trigger.method} in {nameof(TriggeredBehaviour)}");

            method.Invoke(this, Array.Empty<object>());
        }

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
