using System.ComponentModel;
using Primer.Animation;
using Primer.Table;
using Primer.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Grid
{
    [TrackClipType(typeof(CellDisplacerClip))]
    [TrackBindingType(typeof(GridGenerator))]
    public class GridTrack : PrimerTrack
    {
        public AnimationCurve curve = IPrimerAnimation.cubic;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var playable = ScriptPlayable<GridMixer>.Create(graph, inputCount);
            playable.GetBehaviour().curve = curve;
            return playable;
        }
    }
}
