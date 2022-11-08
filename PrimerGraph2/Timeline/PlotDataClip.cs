using System;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [Serializable] public class PlotDataClip : PrimerClip<PlotData>
    {
        public override ClipCaps clipCaps => ClipCaps.Blending;
    }
}
