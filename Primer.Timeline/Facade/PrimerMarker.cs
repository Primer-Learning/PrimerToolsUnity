using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public class PrimerMarker : Marker, INotification, INotificationOptionProvider
    {
        public bool retroactive = true;
        public bool emitInEditor = true;

        NotificationFlags INotificationOptionProvider.flags =>
            (retroactive ? NotificationFlags.Retroactive: default(NotificationFlags)) |
            (emitInEditor ? NotificationFlags.TriggerInEditMode : default(NotificationFlags));

        public PropertyName id => new();
    }
}
