using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Primer;
using UnityEngine;



public class GraphPoint : PrimerBehaviour
{
}


[ExecuteInEditMode]
public class PointData2 : ObjectGenerator
{
    Graph2 _graph;
    Graph2 graph => _graph ?? (_graph = GetComponent<Graph2>());
    Guid id = Guid.NewGuid();

    public PrimerBehaviour pointPrefab;
    public List<Vector3> points = new List<Vector3>();
    List<GraphPoint> pointObjects = new List<GraphPoint>();

    public override void UpdateChildren() {
        var (add, remove, update) = SynchronizeLists(
            points,
            pointObjects,
            (point, obj) => obj.transform.position == point
        );

        foreach (var gp in remove) {
            pointObjects.Remove(gp);
            if (gp) gp.ShrinkAndDispose();
        }

        foreach (var point in add) {
            var pb = Create(pointPrefab, point);
            pb.transform.localPosition = point;
            var gp = pb.gameObject.AddComponent<GraphPoint>();
            pointObjects.Add(gp);
            gp.ScaleUpFromZero();
        }
    }

    protected override void OnChildrenRemoved() {
        foreach (var point in pointObjects) {
            point.ShrinkAndDispose();
        }
    }
}
