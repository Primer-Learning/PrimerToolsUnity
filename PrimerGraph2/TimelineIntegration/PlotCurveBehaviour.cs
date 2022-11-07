using System;
using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace Primer.Graph
{
    [Serializable]
    public class PlotCurveBehaviour : PrimerPlayable<Graph2>
    {
        [Tooltip("Starting point for the line. y axis is ignored")]
        public Vector3 start = Vector3.zero;
        [Tooltip("Ending point for the line. y axis is ignored")]
        public Vector3 end = Vector3.one * 10;

        public int resolution = 200;
        public Polyline prefab;

        [SerializeReference]
        public Curve curve = new LinearCurve();

        internal List<PolylinePoint> points;
        Polyline line;

        int lastResolution;
        Vector3 lastStart;
        Vector3 lastEnd;

        public override void Start(Graph2 graph) {
            prefab ??= graph.linePrefab;
            line = Object.Instantiate(prefab, graph.domain);
            lastStart = Vector3.zero;
        }

        public override void Stop() {
            line?.gameObject?.Dispose();
        }

        public override void Frame(Graph2 graph, Playable playable, FrameData info) {
            if (start != lastStart) {
                line.transform.position = Vector3.Scale(start, graph.domain.localScale);
            }

            if (resolution != lastResolution || start != lastStart || end != lastEnd) {
                CalculatePoints();
            }

            var time = playable.GetTime() / duration;
            var toRender = Mathf.CeilToInt(resolution * (float)time);

            line.points = points.GetRange(0, toRender + 1);
            line.meshOutOfDate = true;

            lastResolution = resolution;
            lastStart = start;
            lastEnd = end;
        }

        void CalculatePoints() {
            var step = (end - start) / resolution;
            points = new List<PolylinePoint>();

            for (var i = 0; i <= resolution; i++) {
                var point = i * step;
                point.y = curve.Evaluate(point.x, point.z);
                points.Add(new PolylinePoint(point));
            }
        }
    }
}
