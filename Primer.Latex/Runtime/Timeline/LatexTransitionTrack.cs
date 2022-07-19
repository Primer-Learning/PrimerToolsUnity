using System.ComponentModel;
using UnityEngine;
using UnityEngine.Timeline;

namespace LatexRenderer.Timeline
{
    [DisplayName("Primer Learning/Latex Transition")]
    [TrackClipType(typeof(ScaleDownClip))]
    public class LatexTransitionTrack : TrackAsset
    {
        [Tooltip("The text we will be transitioning *from*.")]
        public ExposedReference<GameObject> before;

        [Tooltip("The text we will be transitioning *to*.")]
        public ExposedReference<GameObject> after;
    }
}