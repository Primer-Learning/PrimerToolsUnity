using System.ComponentModel;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [DisplayName("Primer Learning / Graph Track")]
    [TrackClipType(typeof(PlotCurveClip))]
    [TrackBindingType(typeof(Graph2))]
    public class GraphTrack : PrimerTrack {}
}
