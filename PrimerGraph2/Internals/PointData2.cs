using System.Collections.Generic;
using Primer;
using Primer.Graph;
using UnityEngine;
using Object = UnityEngine.Object;

[ExecuteInEditMode]
public class PointData2 : ObjectGenerator
{
    Graph2 _graph;
    Graph2 graph => _graph ?? (_graph = GetComponent<Graph2>());

    public PointAnimation pointAnimation = PointAnimation.ScaleUpFromZero;
    public PointController2 pointPrefab;
    public List<Vector3> points = new List<Vector3>();
    List<GraphPoint> pointObjects = new List<GraphPoint>();

    public void AddPoint(string xyz) {
        var parts = xyz.Split('-');
        var x = float.Parse(parts[0]);
        var y = float.Parse(parts[1]);
        var z = float.Parse(parts[2]);
        var vec = new Vector3(x, y, z);
        AddPoint(vec);
    }
    public void AddPoint(Vector3 pos) {
        points.Add(pos);
        UpdateChildren();
    }


    public override void UpdateChildren() {
        var (add, remove, update) = SynchronizeLists(
            points,
            pointObjects,
            (point, obj) =>  obj.domain == point,
            obj => obj.gameObject
        );

        foreach (var point in remove) {
            pointObjects.Remove(point);
            if (point) point.gameObject.ShrinkAndDispose();
        }

        foreach (var domain in add) {
            var pos = graph.DomainToPosition(domain);
            var pb = Create(pointPrefab, pos);
            var gp = new GraphPoint() { domain = domain, gameObject = pb };

            pb.appearance = pointAnimation;
            pointObjects.Add(gp);
        }
    }

    protected override void OnChildrenRemoved() {
        foreach (var point in pointObjects) {
            point.gameObject.ShrinkAndDispose();
        }
    }

    class GraphPoint : Object
    {
        public Vector3 domain;
        public PrimerBehaviour gameObject;

        public static implicit operator bool(GraphPoint point) {
            return point.gameObject;
        }
    }
}
