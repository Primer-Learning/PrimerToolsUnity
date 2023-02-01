using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [ExecuteAlways]
    public abstract class TriggeredBehaviour : MonoBehaviour, INotificationReceiver
    {
        public virtual void Prepare() {}

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not TriggerMarker trigger)
                return;

            if (trigger.method is null) {
                this.Log("Method to trigger is null. Please inspect the marker to ensure a method is selected.");
                return;
            }

            var method = GetType().GetMethod(trigger.method);

            if (method is null)
                throw new Exception($"There is no method {trigger.method} in {GetType().FullName}");

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
