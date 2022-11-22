using System;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [Serializable] public class PlottedDataClip : PrimerClip<PlotData>
    {
        public override ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
    }
}
