using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    public abstract class PrimerMarker : Marker, INotification, INotificationOptionProvider
    {
        [Tooltip("Whether the marker should be executed if the timeline is played after this marker's position")]
        public bool retroactive = true;
        [Tooltip("Prevent this marker to be executed twice")]
        public bool emitOnce = false;
        [Tooltip("Whether this marker should be executed in edit mode")]
        public bool emitInEditor = true;

        NotificationFlags INotificationOptionProvider.flags =>
            (retroactive ? NotificationFlags.Retroactive: default(NotificationFlags)) |
            (emitOnce ? NotificationFlags.TriggerOnce: default(NotificationFlags)) |
            (emitInEditor ? NotificationFlags.TriggerInEditMode : default(NotificationFlags));

        public PropertyName id => new();
    }
}
