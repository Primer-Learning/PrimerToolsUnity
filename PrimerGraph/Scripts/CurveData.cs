using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 2D curve that can exist in a <see cref="Graph"/>
/// </summary>
public class CurveData : MonoBehaviour
{
    int curveResolution = 200;
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private InterpolationMode interpolationMode = InterpolationMode.None;
    public Graph plot;
    Vector3[] dataPoints;
    public bool animationDone = true;

    public void SetColor(Color c) {
        foreach (LineRenderer lr in lineRenderers) {
            lr.material.SetColor("_EmissionColor", c);
        }
    }
    public void SetLineWidth(float width) {
        if (interpolationMode == InterpolationMode.None) {
            LineRenderer lineRenderer = lineRenderers[0];
            lineRenderer.widthMultiplier = width;
        }
        else if (interpolationMode == InterpolationMode.Linear) {
            for (int i = 0; i < dataPoints.Length - 1; i++) {
                LineRenderer lr = lineRenderers[i];
                lr.widthMultiplier = width;
            }
        }
    }
    public void setDataPoints(Func<float, float> newFunction)
    {
        var p = new Vector3[curveResolution];
        for (int i = 0; i < curveResolution; i++)
        {
            p[i] = new Vector3();
            p[i].x = i / ((float)curveResolution - 1) * (plot.xMax - plot.xMin) + plot.xMin;
            p[i].y = newFunction(p[i].x);
        }
        Refresh(p);
    }
    public void setDataPoints(List<float> points) {
        interpolationMode = InterpolationMode.Linear;

        var p = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            p[i] = new Vector3();
            p[i].x = i / ((float)points.Count - 1) * (plot.xMax - plot.xMin) + plot.xMin;
            p[i].y = points[i];
        }
        Refresh(p);
    }

    /// <summary>
    /// Animates the curve to a new curve, morphing along the y-axis
    /// </summary>
    /// <param name="values">The new curve to animate to. Must have the same number of values as the existing curve</param>
    public void AnimateToNewCurve(Vector3[] values, float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            if (animationDone) {
                StartCoroutine(animateCurveTransition(dataPoints, values, duration, ease));
            }
            else {
                ForceStopAnimation();
            }
        }
        else {
            Refresh(values);
        }
    }

    /// <summary>
    /// Animates the curve to a new curve, morphing along the y-axis
    /// </summary>
    /// <param name="newCurve">The new curve to animate to</param>
    public void AnimateToNewCurve(Func<float, float> newCurve, float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (newCurve == null) {
            Debug.LogError("No function defined!");
            //flatten the plane
            newCurve = delegate (float x) { return 0; };
        }
        var p = new Vector3[curveResolution];
        for (int i = 0; i < curveResolution; i++) {
            p[i] = new Vector3();
            p[i].x = i / ((float)curveResolution - 1) * (plot.xMax - plot.xMin) + plot.xMin;
            p[i].y = newCurve(p[i].x);
        }
        AnimateToNewCurve(p, duration, ease);
    }

    public void DrawLineAnimation(float duration = 1.5f, EaseMode ease = EaseMode.Cubic)
    {
        if (interpolationMode == InterpolationMode.Linear) {
            ease = EaseMode.None;
        }
        if (animationDone) {
            StartCoroutine(AnimateThroughPoints(duration, ease));
        }
    }

    IEnumerator AnimateThroughPoints(float duration, EaseMode ease) {
        animationDone = false;
        List<Vector3[]> finalPointsSets = new List<Vector3[]>();
        foreach (LineRenderer lr in lineRenderers) {
            Vector3[] finalPoints = new Vector3[lr.positionCount];
            lr.GetPositions(finalPoints); //This is, apparently, how this method works.
            finalPointsSets.Add(finalPoints);
            lr.positionCount = 0;
        }
        var init = Time.time;
        float segDur = duration / lineRenderers.Count;
        float timeSoFar = 0;
        for (int i = 0; i < lineRenderers.Count; i++) {
            LineRenderer lr = lineRenderers[i];
            while (Time.time - init < timeSoFar + segDur) {
                    float t = (Time.time - init - timeSoFar) / segDur;
                    t = Helpers.ApplyNormalizedEasing(t, ease);
                    float nextPointIndex = t * finalPointsSets[i].Length;
                    while (lr.positionCount < nextPointIndex) {
                        lr.positionCount += 1;
                        lr.SetPosition(lr.positionCount - 1, finalPointsSets[i][lr.positionCount - 1]);
                    }
                yield return new WaitForEndOfFrame();
            }
            timeSoFar += segDur;
            lineRenderers[i].positionCount = finalPointsSets[i].Length;
            lineRenderers[i].SetPositions(finalPointsSets[i]);
        }
        animationDone = true;
    }

    public void WipeCurveAnimation(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        if (animationDone) {
            StartCoroutine(AnimatePointsAway(duration, ease));
        }
    }
    IEnumerator AnimatePointsAway(float duration, EaseMode ease) {
        animationDone = false;
        List<Vector3[]> finalPointsSets = new List<Vector3[]>();
        foreach (LineRenderer lr in lineRenderers) {
            Vector3[] finalPoints = new Vector3[lr.positionCount];
            lr.GetPositions(finalPoints); //This is, apparently, how this method works.
            finalPointsSets.Add(finalPoints);
            // lr.positionCount = 0;
        }
        var init = Time.time;
        float segDur = duration / lineRenderers.Count;
        float timeSoFar = 0;
        for (int i = 0; i < lineRenderers.Count; i++) {
            LineRenderer lr = lineRenderers[i];
            while (Time.time - init < timeSoFar + segDur) {
                    float t = (Time.time - init - timeSoFar) / segDur;
                    t = Helpers.ApplyNormalizedEasing(t, ease);
                    float nextPointIndex = t * finalPointsSets[i].Length;
                    while (lr.positionCount < nextPointIndex) {
                        lr.positionCount -= 1;
                        finalPointsSets[i] = finalPointsSets[i].Skip(1).ToArray();
                        lr.SetPositions(finalPointsSets[i]);
                    }
                yield return new WaitForEndOfFrame();
            }
            timeSoFar += segDur;
            lineRenderers[i].positionCount = 0;
            lineRenderers[i].SetPositions(finalPointsSets[i]);
        }
        animationDone = true;
    }

    /// <summary>
    /// Force the playing animation to stop
    /// </summary>
    public void ForceStopAnimation() {
        StopAllCoroutines();

        animationDone = true;
    }

    /// <summary>
    /// Sets the number of points that the curve will display using. Only applies when specifing curve as a function
    /// </summary>
    /// <param name="resolution">Number of points on the curve</param>
    private void SetCurveResolution(int resolution) {
        curveResolution = resolution;
    }
    public void RefreshData() {
        if (interpolationMode == InterpolationMode.None) {
            LineRenderer lineRenderer = lineRenderers[0];
            if (lineRenderer == null) {
                Debug.LogError("Line Renderer Not defined");
                return;
            }
            int pointCount;
            if (animationDone) {
                pointCount = dataPoints.Length;
            }
            else {
                pointCount = lineRenderer.positionCount;
            }
            var points = new Vector3[pointCount];
            for (int i = 0; i < pointCount; i++) {
                points[i].x = dataPoints[i].x / (plot.xMax - plot.xMin) * plot.xLengthMinusPadding;
                points[i].y = dataPoints[i].y / (plot.yMax - plot.yMin) * plot.yLengthMinusPadding;
                points[i].z = dataPoints[i].z / (plot.zMax - plot.zMin) * plot.zLengthMinusPadding;
            }
            lineRenderer.positionCount = pointCount;
            lineRenderer.SetPositions(points);
            //float y = plot.yAxisLength;
            //lineRenderer.widthMultiplier = plot.GetIntrinsicScale().y; //Should be uniform, but y thickness is most relevant
        }

        if (interpolationMode == InterpolationMode.Linear) {
            //Effectively set curve resolution to be equal to a multiple of the number of segments
            //Exclude final point from calculation because range is inclusive
            int pointsPerSegment = Mathf.CeilToInt((float)(curveResolution - 1) / lineRenderers.Count);
            for (int i = 0; i < dataPoints.Length - 1; i++) {
                LineRenderer lr = lineRenderers[i];
                var points = new Vector3[pointsPerSegment + 1]; //+1 because endpoints overlap with next segment
                for (int j = 0; j < points.Length; j++) {
                    //Interpolate between data points
                    points[j] = Vector3.Lerp(dataPoints[i], dataPoints[i + 1], (float)j / pointsPerSegment);

                    //Normalize to bounds
                    points[j].x = points[j].x / (plot.xMax - plot.xMin) * plot.xLengthMinusPadding;
                    points[j].y = points[j].y / (plot.yMax - plot.yMin) * plot.yLengthMinusPadding;
                    points[j].z = points[j].z / (plot.zMax - plot.zMin) * plot.zLengthMinusPadding;
                }
                lr.positionCount = points.Length;
                lr.SetPositions(points);
                // lr.widthMultiplier = plot.GetIntrinsicScale().y; //Should be uniform, but y thickness is most relevant
            }
        }
    }

    private void OnApplicationQuit() {
        ForceStopAnimation();
    }

    private void Refresh(Vector3[] values) {
        dataPoints = values;
        RefreshData();
    }

    private void Zero() {
        transform.localRotation = Quaternion.identity; //keep this so we can use lossyScale
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero; //don't let you drag this by accident
    }


    private IEnumerator animateCurveTransition(Vector3[] from, Vector3[] to, float duration, EaseMode ease) {
        //Debug.Log(duration);
        animationDone = false;
        var init = Time.time;
        if(from == null || from.Length != to.Length) {
            Debug.LogWarning("cannot animate to curve of different length! Setting this to be the new curve");
            Refresh(to);
            animationDone = true;
            DrawLineAnimation(duration);
            yield break;
        }

        while (Time.time - init < duration) {
            var tempPoints = new Vector3[from.Length];
            float t = (Time.time - init) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            for (int i = 0; i < tempPoints.Length; i++) {
                tempPoints[i] = Vector3.Lerp(from[i], to[i], t);
            }
            Refresh(tempPoints);
            yield return new WaitForEndOfFrame();
        }
        Refresh(to);
        animationDone = true;
    }

    private enum InterpolationMode {
        //Not really a good name for what this currently does.
        //It sets whether we're dealing with only a few points, each connected by a separate linerenderer
        None,
        Linear
    }
}
