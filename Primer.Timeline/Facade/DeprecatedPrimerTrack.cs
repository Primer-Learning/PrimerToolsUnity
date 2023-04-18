using System;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    [Obsolete("Use PrimerTrack instead")]
    public abstract class DeprecatedPrimerTrack : TrackAsset
    {
        // TODO: GatherProperties from PrimerPlayables that extend IPropertyModifier
    }
}
