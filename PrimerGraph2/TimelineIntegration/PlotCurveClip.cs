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
        public PlotCurveBehaviour template = new();

        public float Curve(float x) => Mathf.Log(x + 1, 2);

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner) {
            template.SetClip(this);

            var playable = ScriptPlayable<PlotCurveBehaviour>.Create(graph, template);

            // playable.GetBehaviour().prefab = prefab;

            // var behaviour = playable.GetBehaviour();
            // adapter.resolver = graph.GetResolver();
            // behaviour.adapter = adapter;

            return playable;
        }
    }
}
