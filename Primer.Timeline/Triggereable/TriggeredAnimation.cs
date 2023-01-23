using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    public class PrepareMethodAttribute : Attribute {}

    [ExecuteAlways]
    public abstract class TriggeredAnimation : MonoBehaviour, INotificationReceiver
    {
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not TriggerMarker trigger || trigger.method is null)
                return;

            var type = GetType();
            var method = type.GetMethod(trigger.method);

            if (method is null)
                throw new Exception($"No method {trigger.method} in {nameof(TriggeredAnimation)}");

            method.Invoke(this, Array.Empty<object>());
        }

        protected static async UniTask Wait(int milliseconds)
        {
            if (Application.isPlaying)
                await UniTask.Delay(milliseconds);
        }
    }
}
