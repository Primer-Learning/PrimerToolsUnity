using System;
using System.Collections.Generic;
using Primer.Timeline;
using Shapes;
using UnityEngine;

namespace Primer.Graph
{
    [Serializable]
    public class PlotData : PrimerPlayable, ILineBehaviour, ISurfaceBehaviour
    {
        public List<Vector3> points = new();

        ILine ILineBehaviour.Points
        {
            get {
                var list = new List<PolylinePoint>();
                var count = points.Count;

                for (var i = 0; i < count; i++) {
                    list.Add(new PolylinePoint(points[i]));
                }

                return new SimpleLine(list);
            }
        }

        IGrid ISurfaceBehaviour.Grid => new ContinuousGrid(points.ToArray());
    }
}
