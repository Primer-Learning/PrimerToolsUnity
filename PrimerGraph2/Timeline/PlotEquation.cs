using System;
using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Graph
{
    [Serializable]
    public class PlotEquation : PrimerPlayable<Polyline>, ILineBehaviour
    {
        [SerializeReference]
        public ParametricEquation equation = new LinearEquation();
        public int resolution = 100;

        public List<PolylinePoint> points { get; private set; }
        int lastResolution;
        ParametricEquation lastEquation;

        public override void Start(Polyline line) {}

        public override void Stop(Polyline line) {}

        public override void Frame(Polyline line, Playable playable, FrameData info) {
            if (resolution == lastResolution && equation == lastEquation) return;

            CalculatePoints();

            lastResolution = resolution;
            lastEquation = equation;
        }

        void CalculatePoints() {
            // resolution represents the amount of segments
            // points = segments + 1
            var pointsCount = resolution + 1;

            var points = new List<PolylinePoint>(pointsCount);

            for (var i = 0; i < pointsCount; i++) {
                var t = i / (float)pointsCount;
                var vector = equation.Evaluate(t);
                points.Add(new PolylinePoint(vector));
            }

            this.points = points;
        }
    }
}
