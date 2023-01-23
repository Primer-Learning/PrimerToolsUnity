using System.ComponentModel;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    [DisplayName("Primer / Trigger Track")]
    [TrackBindingType(typeof(TriggeredAnimation))]
    public class TriggerTrack : MarkerTrack
    {
    }
}
