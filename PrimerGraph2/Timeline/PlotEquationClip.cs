using System;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [Serializable] public class PlotEquationClip : PrimerClip<PlotEquation>
    {
        public override ClipCaps clipCaps => ClipCaps.Blending;
    }
}
