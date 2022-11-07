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

        [Tooltip("Starting point for the line. y axis is ignored")]
        public Vector3 start = Vector3.zero;
        [Tooltip("Ending point for the line. y axis is ignored")]
        public Vector3 end = Vector3.one * 10;

        public int resolution = 200;
        public Polyline prefab;

        [SerializeReference]
        public Curve curve = new LinearCurve();

        Polyline line;
        Vector3 lastStart;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData) {
            if (playerData is not Graph2 graph) {
                throw new TimelineException("PlotCurveBehaviour can only be used in Graph2 tracks");
            }

            if (!line) {
                Initialize(graph);
            }

            if (start != lastStart) {
                lastStart = start;
                line.transform.position = Vector3.Scale(start, graph.domain.localScale);
            }

            var pointsToDraw = resolution + 1;
            var time = playable.GetTime() / duration;
            var step = (end - start) / pointsToDraw;
            var toRender = Mathf.CeilToInt(pointsToDraw * (float)time);
            var points = new List<PolylinePoint>();

            for (var i = 0; i < toRender; i++) {
                var point = i * step;
                point.y = curve.Evaluate(point.x, point.z);
                points.Add(new PolylinePoint(point));
            }

            line.points = points;
            line.meshOutOfDate = true;
        }

        void Initialize(Graph2 graph) {
            prefab ??= graph.linePrefab;
            line = Object.Instantiate(prefab, graph.domain);
            lastStart = Vector3.zero;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info) {
            line?.gameObject?.Dispose();
        }
    }
}
