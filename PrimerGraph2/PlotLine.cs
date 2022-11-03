using System.Collections.Generic;
using Primer;
using Primer.Timeline;
using Shapes;
using UnityEditor;
using UnityEngine;

public class PlotLine : ScrubbableAdapter
{
    public int resolution = 200;
    public int length = 10;
    // public float step = 0.1f;
    public Polyline prefab;

    public ExposedReference<Transform> _parent;
    Transform parent => Resolve(_parent);

    Polyline line;

    public virtual float Curve(float x) {
        if (x == 5f) return 0;
        return Mathf.Log(x + 1, 2);
    }

    public override void Update(UpdateArgs args) {
        if (!line) {
            Debug.Log("LINE NOT INITIALIZED");
            Prepare();
        }

        var totalPointsCount = resolution;
        var step = length / (float)totalPointsCount;
        var toRender = Mathf.RoundToInt(totalPointsCount * (float)args.time);
        var points = new List<PolylinePoint>();

        for (var i = 0; i < toRender; i++) {
            var x = i * step;
            var vec = new Vector2(x, Curve(x));
            points.Add(new PolylinePoint(vec));
        }

        line.points = points;
        line.meshOutOfDate = true;
    }

    public override void Prepare() {
        line = Object.Instantiate(prefab, parent);
    }

    public override void Cleanup() {
        Debug.Log("CLEANUP");
        line?.gameObject?.Dispose();
    }

    public override void RegisterPreviewingProperties(PropertyRegistrar registrar) {
        Debug.Log("REGISTER");
    }
}
