using System.ComponentModel;
using Primer.Timeline;
using UnityEngine.Timeline;

namespace Primer.Latex
{
    [DisplayName("Primer / Latex track")]
    [TrackClipType(typeof(TokenReplacementClip))]
    [TrackBindingType(typeof(LatexRenderer))]
    public class GraphLineTrack : PrimerTrack
    {
    }
}
