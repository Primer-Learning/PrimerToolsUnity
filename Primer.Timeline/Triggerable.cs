using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Timeline
{
    [ExecuteAlways]
    public abstract class Triggerable : AsyncMonoBehaviour, INotificationReceiver
    {
        public virtual void Prepare() {}
        public virtual void Cleanup() {}


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
    }
}
