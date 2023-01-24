using Primer.Math;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Table
{
    public class CellDisplacerClip : PlayableAsset, ITimelineClipAsset
    {
        public bool multiplyExistingPosition;

        [SerializeReference]
        public ParametricEquation equation = new CellPlacerEquation();

        public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CellDisplacerBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();

            behaviour.equation = equation;
            behaviour.multiplyExistingPosition = multiplyExistingPosition;

            return playable;
        }
    }
}
