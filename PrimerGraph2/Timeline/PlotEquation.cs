using System;
using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class PlotEquation : PrimerPlayable, ILineBehaviour, ISurfaceBehaviour
    {
        [SerializeReference]
        public ParametricEquation equation = new LinearEquation();

        [Min(1)]
        public int resolution = 100;

        int lastResolution;
        ParametricEquation lastEquation;

        List<PolylinePoint> pointsCache;
        List<PolylinePoint> ILineBehaviour.Points
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

        Vector3[] points2dCache;
        Vector3[] ISurfaceBehaviour.Points
        {
            get {
                if (resolution != lastResolution || equation != lastEquation) {
                    RecalculatePoints2D();
                    lastResolution = resolution;
                    lastEquation = equation;
                }

                return points2dCache;
            }
        }

        void RecalculatePoints() {
            // resolution represents the amount of segments
            // points = segments + 1
            var pointsCount = resolution + 1;

            var points = new List<PolylinePoint>(pointsCount);

            for (var i = 0; i < pointsCount; i++) {
                var t = i / (float)resolution;
                var vector = equation.Evaluate(t);
                points.Add(new PolylinePoint(vector));
            }

            pointsCache = points;
        }

        void RecalculatePoints2D() {
            // resolution represents the amount of segments
            // points = segments + 1
            var pointsCount = resolution + 1;

            var points = new Vector3[pointsCount * pointsCount];

            for (var i = 0; i < pointsCount; i++) {
                for (var j = 0; j < pointsCount; j++) {
                    var t = i / (float)resolution;
                    var u = j / (float)resolution;
                    points[i * pointsCount + j] = equation.Evaluate(t, u);
                }
            }

            points2dCache = points;
        }
    }
}
