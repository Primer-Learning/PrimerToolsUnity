using System;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Graph
{
    [Serializable]
    public class PlotCurveClip : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;
        public Polyline prefab;

        [SerializeReference]
        public Curve curve = new LinearCurve();
        public PlotCurveBehaviour template = new();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            template.prefab = prefab;
            template.Curve = curve;

            return ScriptPlayable<PlotCurveBehaviour>.Create(graph, template);
        }
    }
}
