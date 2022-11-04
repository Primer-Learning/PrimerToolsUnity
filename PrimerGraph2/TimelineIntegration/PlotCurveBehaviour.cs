using System;
using System.Collections.Generic;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Graph
{
    [Serializable]
    public class PlotCurveBehaviour : PlayableBehaviour
    {
        internal double duration;
        public int resolution = 200;
        public int length = 10;

        public Curve Curve { get; set; }
        public Polyline prefab { get; set; }

        Polyline line;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            if (!line) {
                if (playerData is Graph2 graph)
                    Initialize(graph);
                else
                    throw new TimelineException("PlotCurveBehaviour can only be used in Graph2 tracks");
            }

            var time = playable.GetTime() / duration;
            var totalPointsCount = resolution;
            var step = length / (float)totalPointsCount;
            var toRender = Mathf.RoundToInt(totalPointsCount * (float)time);
            var points = new List<PolylinePoint>();

            for (var i = 0; i < toRender; i++) {
                var x = i * step;
                var vec = new Vector2(x, Curve.Evaluate(x));
                points.Add(new PolylinePoint(vec));
            }

            line.points = points;
            line.meshOutOfDate = true;
        }

        void Initialize(Graph2 graph) {
            line = Object.Instantiate(prefab, graph.domain);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            line?.gameObject?.Dispose();
        }

    }

}
