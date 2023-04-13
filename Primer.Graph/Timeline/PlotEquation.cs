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
        private ParametricEquation lastEquation;

        private int lastResolution;

        private Vector3[] points2dCache;

        private List<PolylinePoint> pointsCache;

        [Min(1)]
        public int resolution = 100;
        ILine ILineBehaviour.Points {
            get {
                if (resolution != lastResolution || equation != lastEquation) {
                    RecalculatePoints();
                    lastResolution = resolution;
                    lastEquation = equation;
                }

                return new SimpleLine(pointsCache);
            }
        }
        IGrid ISurfaceBehaviour.Grid {
            get {
                if (resolution != lastResolution || equation != lastEquation) {
                    RecalculatePoints2D();
                    lastResolution = resolution;
                    lastEquation = equation;
                }

                return new ContinuousGrid(points2dCache);
            }
        }

        private void RecalculatePoints()
        {
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

        private void RecalculatePoints2D()
        {
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
