using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
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
            (point, obj) =>  obj.domain == point,
            obj => obj.gameObject
        );

        foreach (var point in remove) {
            pointObjects.Remove(point);
            if (point) point.gameObject.ShrinkAndDispose();
        }

        foreach (var domain in add) {
            var pos = graph.DomainToPosition(domain);
            var pb = Create(pointPrefab, new Vector3(pos.x, 0, pos.z));
            var gp = new GraphPoint() { domain = domain, gameObject = pb };

            AnimatePointAppearance(pb, pos);
            pointObjects.Add(gp);
        }
    }

    protected override void OnChildrenRemoved() {
        foreach (var point in pointObjects) {
            point.gameObject.ShrinkAndDispose();
        }
    }

    async void AnimatePointAppearance(PrimerBehaviour point, Vector3 target) {
        await point.ScaleUpFromZeroAwaitable();
        await Task.Delay(500);
        await point.MoveTo(target);
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
