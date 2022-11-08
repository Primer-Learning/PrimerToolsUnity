using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Timeline;
using Shapes;
using UnityEngine;
using UnityEngine.Playables;

namespace Primer.Graph
{
    [Serializable]
    public class PlotData : PrimerPlayable<Polyline>, ILineBehaviour
    {
        public List<Vector3> _points = new();
        public List<PolylinePoint> points => _points.Select(x => new PolylinePoint(x)).ToList();

        public override void Start(Polyline trackTarget) {}

        public override void Stop(Polyline trackTarget) {}

        public override void Frame(Polyline trackTarget, Playable playable, FrameData info) {}
    }
}
