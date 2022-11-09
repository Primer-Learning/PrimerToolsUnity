using System;
using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class PlotEquation : PrimerPlayable, ILineBehaviour
    {
        [SerializeReference]
        public ParametricEquation equation = new LinearEquation();
        public int resolution = 100;

        int lastResolution;
        ParametricEquation lastEquation;

        List<PolylinePoint> pointsCache;
        public List<PolylinePoint> Points
        {
            get {
                if (resolution != lastResolution || equation != lastEquation) {
                    RecalculatePoints();
                    lastResolution = resolution;
                    lastEquation = equation;
                }

                return pointsCache;
            }
        }

        void RecalculatePoints() {
            // resolution represents the amount of segments
            // points = segments + 1
            var pointsCount = resolution + 1;

            var points = new List<PolylinePoint>(pointsCount);

            for (var i = 0; i < pointsCount; i++) {
                var t = i / (float)pointsCount;
                var vector = equation.Evaluate(t);
                points.Add(new PolylinePoint(vector));
            }

            pointsCache = points;
        }
    }
}
