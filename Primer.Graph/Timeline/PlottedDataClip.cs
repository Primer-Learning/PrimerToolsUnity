using System;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [Serializable] public class PlottedDataClip : DeprecatedPrimerClip<PlotData>
    {
        public override ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
    }
}
