using Primer;
using Primer.Timeline;
using UnityEngine;

public class PlotLine : ScrubbableAdapter
{
    public int length = 10;
    public float step = 0.1f;
    public LineRenderer prefab;

    public ExposedReference<Transform> _parent;
    Transform parent => Resolve(_parent);

    LineRenderer line;

    public virtual float Curve(float x) {
        if (x == 5) return 0;
        return Mathf.Log(x + 1, 2);
    }

    public override void Update(UpdateArgs args) {
        if (!line) {
            Debug.Log("LINE NOT INITIALIZED");
            Prepare();
        }

        var totalPointsCount = length * (1 / step);
        var toRender = Mathf.RoundToInt(totalPointsCount * (float)args.time);
        var points = new Vector3[toRender];

        for (var i = 0; i < toRender; i++) {
            var x = i * step;
            points[i] = new Vector3(x, Curve(x), 0);
        }

        line.positionCount = toRender;
        line.SetPositions(points);
    }

    public override void Prepare() {
        line = Object.Instantiate(prefab, parent);
    }

    public override void Cleanup() {
        Debug.Log("CLEANUP");
        line.gameObject.Dispose();
    }

    public override void RegisterPreviewingProperties(PropertyRegistrar registrar) {
        Debug.Log("REGISTER");
    }
}
