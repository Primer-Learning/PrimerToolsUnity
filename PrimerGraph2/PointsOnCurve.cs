using System.Collections.Generic;
using Primer;
using Primer.Timeline;
using UnityEngine;

public class PointsOnCurve : ScrubbableAdapter
{
    public ExposedReference<Transform> _parent;
    Transform parent => Resolve(_parent);

    public int amount = 10;
    public EaseMode ease;
    public GameObject pointPrefab;
    public float prefabScale = 1f;
    public float offset = 0;
    public float step = 1;

    List<GameObject> points;

    public virtual float Curve(float x) => x;

    public override void Update(UpdateArgs args) {
        if (Util.IsNull(pointPrefab)) return;

        points ??= new List<GameObject>();

        // resolve the property only once
        var parent = this.parent;

        for (var i = 0; i < amount && points.Count < i + 1; i++) {
            var newPoint = parent is null
                ? Object.Instantiate(pointPrefab)
                : Object.Instantiate(pointPrefab, parent);

            newPoint.transform.localPosition = new Vector3(i, 0, 0);
            points.Add(newPoint);
        }

        var easedTime = Easing.ApplyNormalizedEasing((float)args.time, ease);

        UpdateScale(easedTime);
        UpdatePosition(easedTime);
    }

    void UpdateScale(float easedTime) {
        // scalingTime will range from 0 to 11 over the course of the animation
        // and nothing will happen from 10 to 11
        var scalingTime = easedTime * (amount + 1);
        var parentScale = parent?.localScale ?? Vector3.one;
        var start = Vector3.zero;

        for (var i = 0; i < amount; i++) {
            var t = Easing.ApplyNormalizedEasing(scalingTime - i, ease);
            var end = new Vector3(
                1 / parentScale.x * prefabScale,
                1 / parentScale.y * prefabScale,
                1 / parentScale.z * prefabScale
            );

            points[i].transform.localScale = Vector3.Lerp(start, end, t);
        }
    }

    void UpdatePosition(float easedTime) {
        // movingTime will range from -1 to 10 over the animation
        // and nothing will happen between -1 and 0.
        var movingTime = easedTime * (amount + 1) - 1;

        for (var i = 0; i < amount; i++) {
            var t = Easing.ApplyNormalizedEasing(movingTime - i, ease);
            var x = offset + i * step;
            var start = new Vector3(x, 0, 0);
            var end = new Vector3(x, Curve(x), 0);

            points[i].transform.localPosition = Vector3.Lerp(start, end, t);
        }
    }

    public override void Cleanup() {
        while (points.Count > 0) {
            var deadPoint = points[0];
            points.RemoveAt(0);
            Object.DestroyImmediate(deadPoint);
        }
    }

    public override void Prepare() {}

    public override void RegisterPreviewingProperties(PropertyRegistrar registrar) {}
}
