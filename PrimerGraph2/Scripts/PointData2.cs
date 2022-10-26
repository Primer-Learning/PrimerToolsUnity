using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Primer;
using Primer.Graph;
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class PointData2 : ObjectGenerator
{
    Graph2 _graph;
    Graph2 graph => _graph ?? (_graph = GetComponent<Graph2>());

    public PrimerBehaviour pointPrefab;
    public List<Vector3> points = new List<Vector3>();
    List<GraphPoint> pointObjects = new List<GraphPoint>();

    public override void UpdateChildren() {
        var (add, remove, update) = SynchronizeLists(
            points,
            pointObjects,
            (point, obj) => obj.domain == point
        );

        foreach (var point in remove) {
            pointObjects.Remove(point);
            if (point) point.asset.ShrinkAndDispose();
        }

        foreach (var domain in add) {
            var pos = graph.DomainToPosition(domain);
            var pb = Create(pointPrefab, pos);
            var gp = new GraphPoint() { domain = domain, asset = pb };

            pb.ScaleUpFromZero();
            pointObjects.Add(gp);
        }
    }

    protected override void OnChildrenRemoved() {
        foreach (var point in pointObjects) {
            point.asset.ShrinkAndDispose();
        }
    }

    class GraphPoint : Object
    {
        public Vector3 domain;
        public PrimerBehaviour asset;

        public static implicit operator bool(GraphPoint point) {
            return point.asset;
        }
    }
}
