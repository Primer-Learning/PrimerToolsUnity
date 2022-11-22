using System;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [Serializable] public class PlottedEquationClip : PrimerClip<PlotEquation>
    {
        public override ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;
    }
}
