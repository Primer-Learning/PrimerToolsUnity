using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Primer;
using Primer.Graph;
using UnityEngine;

[ExecuteInEditMode]
public class PointData2 : ObjectGenerator
{
    Graph2 _graph;
    Graph2 graph => _graph ?? (_graph = GetComponent<Graph2>());

    public GraphPoint pointPrefab;
    public List<Vector3> points = new List<Vector3>();
    List<GraphPoint> pointObjects = new List<GraphPoint>();

    public override void UpdateChildren() {
        var (add, remove, update) = SynchronizeLists(
            points,
            pointObjects,
            (point, obj) => obj.domainPosition == point
        );

        foreach (var gp in remove) {
            pointObjects.Remove(gp);
            if (gp) gp.ShrinkAndDispose();
        }

        foreach (var point in add) {
            var pos = graph.DomainToPosition(point);
            var gp = Create(pointPrefab, pos);
            gp.domainPosition = point;
            gp.ScaleUpFromZero();
            pointObjects.Add(gp);
        }
    }

    protected override void OnChildrenRemoved() {
        foreach (var point in pointObjects) {
            point.ShrinkAndDispose();
        }
    }
}
