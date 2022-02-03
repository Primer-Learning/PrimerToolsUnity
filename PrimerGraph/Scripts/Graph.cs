using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/* TODO 
 * (Core)
 * Add precision options for tic labels
 * 
 * (When needed)
 * Make padding amount uniform instead of using fraction of each axis length (maybe)
 * Destroy unused tics, animating them to zero scale 
 */  

public class Graph : PrimerObject
{
    //Welcome to the land of way too many public variables.
    [Header("X")] 
    public float xMin;
    public float xMax;
    public float xTicStep;
    public string xAxisLabelString;
    public string xAxisLabelPos;
    public bool xHidden;
    public float xAxisLength;
    //Calculated alignment shortcuts
    public float xLengthMinusPadding;
    public float xAxisOffset;
    public float xDrawnMin;
    public float xAxisThickness;

    [Header("Y")] 
    public float yMin;
    public float yMax;
    public float yTicStep;
    public string yAxisLabelString;
    public string yAxisLabelPos;
    public bool yHidden;
    public float yAxisLength;
    //Calculated alignment shortcuts
    public float yLengthMinusPadding;
    public float yAxisOffset;
    public float yAxisThickness;

    [Header("Z")] 
    public float zMin;
    public float zMax;
    public float zTicStep;
    public string zAxisLabelString;
    public string zAxisLabelPos;
    public bool zHidden;
    public float zAxisLength;
    //Calculated alignment shortcuts
    public float zLengthMinusPadding;
    public float zAxisOffset;
    public float zDrawnMin;
    public float zAxisThickness;

    internal bool manualTicMode;

    public Dictionary<float, string> manualTicsX;
    public Dictionary<float, string> manualTicsY;
    public float ticLabelDistanceVertical;
    public float ticLabelDistanceHorizontal;
    public float ticLabelSize;
    public string arrows;
    public float paddingFraction;
    public bool rightHanded;

    public Axes axesHandler;
    public GameObject arrowPrefab;

    public CurveData curveDataPrefab;
    public LineRenderer lrPrefab;
    public Dictionary<string, CurveData> curves = new Dictionary<string, CurveData>();

    public SurfaceData surfaceDataPrefab;
    public Dictionary<string, SurfaceData> surfaces = new Dictionary<string, SurfaceData>();

    public StackedAreaData stackedAreaDataPrefab;
    public Dictionary<string, StackedAreaData> stackedAreas = new Dictionary<string, StackedAreaData>();

    public PointData pointData;
    public BarDataManager barData = null;
    public BarDataManager barDataPrefab;
    bool labelsFaceCamera = false;
    public List<PrimerObject> allLabels = new List<PrimerObject>();

    void Update() {
        if (labelsFaceCamera) {
            //Vector3 diffVec = Camera.main.transform.position - transform.position;
            //Vector3 rotEuler = Quaternion.LookRotation(diffVec).eulerAngles;
            Quaternion labelRot = Quaternion.Euler(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            foreach (PrimerObject p in allLabels) {
                p.RotateTowardsWithInertia(labelRot, global: true);
            }
        }
    }

    public void Initialize(
        Func<float, float, float>[] initialSurfaceFunctions = null, //Not implemented
        Func<float, float>[] initialCurveFunctions = null, //Not implemented
        Vector3[] initialPoints = null, //Not implemented

        float xMin = 0f,
        float xMax = 10f,
        string xAxisLabelString = "x",
        string xAxisLabelPos = "end", //end or end
        float xAxisLength = 1f, //Axis lengths and are the main tool for controlling size, since changing the graph's scale can warp component objects
        float xTicStep = 2f,
        bool xHidden = false,
        float xAxisThickness = 1,

        float yMin = 0f,
        float yMax = 10f,
        string yAxisLabelString = "y",
        string yAxisLabelPos = "end", //end or along
        float yAxisLength = 1f, //Axis lengths and are the main tool for controlling size, since changing the graph's scale can warp component objects
        float yTicStep = 2f,
        bool yHidden = false,
        float yAxisThickness = 1,
        
        float zMin = 0f,
        float zMax = 10f,
        string zAxisLabelString = "z",
        string zAxisLabelPos = "end", //end or along
        float zAxisLength = 1f, //Axis lengths and are the main tool for controlling size, since changing the graph's scale can warp component objects
        float zTicStep = 2f,
        bool zHidden = false,
        float zAxisThickness = 1,
        
        int ticPrecision = 0, //Not implemented
        float ticLabelDistanceVertical = 0.25f, //For horizontally aligned axes
        float ticLabelDistanceHorizontal = 0.65f, //For the vertically aligned axis
        float ticLabelSize = 1f,
        //Similar for y and z
        float scale = -1, //Scale is float because it will be uniform in all directions to avoid warping. Axis length and tic units are determined elswhere.
        float thickness = 1,
        
        bool manualTicMode = false,
        Dictionary<float, string> manualTicsX = null,

        float paddingFraction = 0.05f, //How much of the total length is allocated to extending axes/arrows beyond the data range
        string arrows = "both",

        bool rightHanded = true
    )
    {   
        if (scale != -1) {
            Debug.LogWarning("Setting scale from Graph.Initialize is deprecated. Use thickness instead. We gotchu this time, tho.");
            thickness = scale;
            xAxisLength *= thickness;
            yAxisLength *= thickness;
            zAxisLength *= thickness;
        }
        this.paddingFraction = paddingFraction;
        this.xMin = xMin;
        this.xMax = xMax;
        this.xAxisLength = xAxisLength / thickness; 
        this.xTicStep = xTicStep;
        this.xAxisLabelString = xAxisLabelString;
        this.xAxisLabelPos = xAxisLabelPos;
        this.xHidden = xHidden; 
        this.xLengthMinusPadding = this.xAxisLength * (1 - 2 * this.paddingFraction);
        //Offset is padding plus the padded/scaled min
        this.xAxisOffset = - this.xAxisLength * this.paddingFraction + this.xMin * this.xLengthMinusPadding / (this.xMax - this.xMin);
        this.xDrawnMin = this.xAxisOffset + this.xAxisLength * paddingFraction;
        this.xAxisThickness = xAxisThickness;

        this.yMin = yMin;
        this.yMax = yMax;
        this.yAxisLength = yAxisLength / thickness;
        this.yTicStep = yTicStep;
        this.yAxisLabelString = yAxisLabelString;
        this.yAxisLabelPos = yAxisLabelPos;
        this.yHidden = yHidden;
        this.yLengthMinusPadding = this.yAxisLength * (1 - 2 * this.paddingFraction);
        //Offset is padding plus the padded/scaled min
        this.yAxisOffset = -this.yAxisLength * this.paddingFraction + this.yMin * this.yLengthMinusPadding / (this.yMax - this.yMin);
        this.yAxisThickness = yAxisThickness;

        this.zMin = zMin;
        this.zMax = zMax;
        this.zAxisLength = zAxisLength / thickness;
        this.zTicStep = zTicStep;
        this.zAxisLabelString = zAxisLabelString;
        this.zAxisLabelPos = zAxisLabelPos;
        this.zHidden = zHidden;
        this.zLengthMinusPadding = this.zAxisLength * (1 - 2 * this.paddingFraction);
        //Offset is padding plus the padded/scaled min
        this.zAxisOffset = -this.zAxisLength * this.paddingFraction + this.zMin * this.zLengthMinusPadding / (this.zMax - this.zMin);
        this.zDrawnMin = this.zAxisOffset + this.zAxisLength * paddingFraction;
        this.yAxisThickness = yAxisThickness;

        this.ticLabelDistanceVertical = ticLabelDistanceVertical;
        this.ticLabelDistanceHorizontal = ticLabelDistanceHorizontal;
        this.ticLabelSize = ticLabelSize;
        this.arrows = arrows;

        this.manualTicMode = manualTicMode;
        this.manualTicsX = manualTicsX;

        this.rightHanded = rightHanded;

        axesHandler.graph = this;
        axesHandler.SetUpAxes();
        
        //Set scale
        //Intrinsic scale determines base size and doesn't vary during animations
        SetIntrinsicScale(thickness);
        transform.localScale = this.intrinsicScale;
    }

    public void PutObjectInGraphSpace(Transform t, Vector3 pos) {
        pos = new Vector3(
            pos.x * this.xLengthMinusPadding / (this.xMax - this.xMin),
            pos.y * this.yLengthMinusPadding / (this.yMax - this.yMin),
            pos.z * this.zLengthMinusPadding / (this.zMax - this.zMin)
        );
        t.parent = transform;
        t.localPosition = pos;
    }

    /// <summary>
    /// Adds a new curve with a given id. If an existing curve with that id already exist, return the existing curve.
    /// </summary>
    /// <param name="id">unique id for the curve, i.e. "Catenary"</param>
    /// <returns></returns>
    public CurveData AddCurve(Func<float, float> functionToPlot, string id) {
        if (curves.ContainsKey(id)) {
            if (curves[id] == null) curves.Remove(id);
            else {
                return curves[id];
            }
        }
        var c = Instantiate(curveDataPrefab, transform);
        var lr = Instantiate(lrPrefab, c.transform);
        lr.transform.parent = c.transform;
        c.lineRenderers.Add(lr);
        c.plot = this;
        c.setDataPoints(functionToPlot);
        curves.Add(id, c);
        return c;
    }
    public CurveData AddCurve(List<float> dataPoints, string id) {
        if (curves.ContainsKey(id)) {
            if (curves[id] == null) curves.Remove(id);
            else {
                return curves[id];
            }
        }
        var c = Instantiate(curveDataPrefab, transform);
        c.name = id;
        for (int i = 0; i < dataPoints.Count - 1; i++) {
            var lr = Instantiate(lrPrefab, c.transform);
            lr.transform.localScale = Vector3.one;
            //Small displacement to bring linerenderer(s) in front of stacked area
            lr.transform.localPosition = new Vector3(0, 0, -0.001f); 
            c.lineRenderers.Add(lr);
        }
        c.plot = this;
        c.setDataPoints(dataPoints);
        curves.Add(id, c);
        return c;
    }
    public CurveData AddCurve(List<int> dataPoints, string id) {
        List<float> asFloat = new List<float>();
        foreach (int num in dataPoints) {
            asFloat.Add((float) num);
        }
        return AddCurve(asFloat, id);
    }

    /// <summary>
    /// Removes a curve with the given id, if it exists.
    /// </summary>
    /// <param name="id">Id of the curve to remove</param>
    /// <returns>Returns true if succesfully removed the curve</returns>
    public bool RemoveCurve(string id) {
        return removeIfExists(id, curves);
    }

    public SurfaceData AddSurface(Func<float, float, float> functionToPlot, string id, bool remakeMesh = false, int edgeResolution = 21) {
        if (surfaces.ContainsKey(id)) {
            if (surfaces[id] == null) surfaces.Remove(id);
            else {
                return surfaces[id];
            }
        }
        var s = Instantiate(surfaceDataPrefab, transform);
        s.plot = this;
        s.mins = new Vector3 (xMin, yMin, zMin);
        s.maxs = new Vector3 (xMax, yMax, zMax);
        s.SetFunction(functionToPlot);
        surfaces.Add(id, s);
        return s;
    }

    public bool RemoveSurface(string id) {
        return removeIfExists(id, surfaces);
    }

    public StackedAreaData AddStackedArea(string id = "None", int pointsPerUnit = 1) {
        if (id == "None") {id = System.Environment.TickCount.ToString();}
        if (stackedAreas.ContainsKey(id)) {
            if (stackedAreas[id] == null) stackedAreas.Remove(id);
            else {
                return stackedAreas[id];
            }
        }
        var s = Instantiate(stackedAreaDataPrefab, transform);
        s.plot = this;
        s.mins = new Vector3 (xMin, yMin, zMin);
        s.maxs = new Vector3 (xMax, yMax, zMax);
        s.pointsPerUnit = pointsPerUnit;
        stackedAreas.Add(id, s);
        //Set this when added because updating clipping number doesn't set y.
        s.RefreshData();
        s.UpdateVisibleRange(s.mins, s.maxs); 
        return s;
    }
    public bool RemoveStackedArea(string id) {
        return removeIfExists(id, stackedAreas);
    }
    public BarDataManager AddBarData() {
        barData = Instantiate(barDataPrefab, transform);
        barData.transform.localPosition = Vector3.zero;
        barData.plot = this;
        barData.AdjustBarSpaceScale();
        return barData;
    }

    private bool removeIfExists<T> (string id, Dictionary<string, T> fromDictionary) where T : MonoBehaviour {
        if (fromDictionary.ContainsKey(id)) {
            var p = fromDictionary[id];
            if (p != null) Destroy(p.gameObject);
            fromDictionary.Remove(id);
            return true;
        }
        return false;
    }

    public void ChangeRangeY(float newMin, float newMax, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(changeRangeY(newMin, newMax, duration, ease));
    }

    private IEnumerator changeRangeY(float newMin, float newMax, float duration, EaseMode ease) {
        float oldMin = this.yMin;
        float oldMax = this.yMax;
        //TODO: Make this work for non-zero mins by changing axis offset and rodcontainer
        float startTime = Time.time;
        while (Time.time <= startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            this.yMin = Mathf.Lerp(oldMin, newMin, t);
            this.yMax = Mathf.Lerp(oldMax, newMax, t);
            yield return null;
            updateYRangeProperties();
        }
        this.yMin = newMin;
        this.yMax = newMax;
        updateYRangeProperties();
    }
    private void updateYRangeProperties() {
        foreach (Tic tic in axesHandler.yTics) {
            tic.SetPosition();
        }
        axesHandler.HandleTicsY();
        if (labelsFaceCamera) {
            UpdateLabelList();//Definitely a more efficient way to do this, but maybe it doesn't matter.
        }
        pointData.RefreshData();
        foreach(var id in curves.Keys) {
            curves[id].RefreshData();
        }
        foreach (var id in surfaces.Keys) {
            surfaces[id].RefreshData();
        }
        foreach (var id in stackedAreas.Keys) {
            var s = stackedAreas[id];
            s.ScaleMesh();
        }
        barData.AdjustBarSpaceScale();
    }

    public void ChangeRangeX(float newMin, float newMax, float? newXLength = null, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(changeRangeX(newMin, newMax, newXLength, duration, ease));
    }

    private IEnumerator changeRangeX(float newMin, float newMax, float? newXLength, float duration, EaseMode ease) {
        float oldXAxisLength = xAxisLength;
        float newXAxisLength = xAxisLength;
        if (newXLength != null) {
            newXAxisLength = (float)newXLength;
        }

        float oldMin = this.xMin;
        float oldMax = this.xMax;
        //TODO: Make this work for non-zero mins by changing axis offset and rodcontainer
        float startTime = Time.time;
        while (Time.time <= startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            this.xMin = Mathf.Lerp(oldMin, newMin, t);
            this.xMax = Mathf.Lerp(oldMax, newMax, t);
            updateXAxisLength(Mathf.Lerp(oldXAxisLength, newXAxisLength, t));
            yield return null;
            updateXRangeProperties();
        }
        this.xMin = newMin;
        this.xMax = newMax;
        updateXAxisLength(newXAxisLength);
        updateXRangeProperties();
    }
    private void updateXAxisLength(float length) {
        this.xAxisLength = length;
        this.xLengthMinusPadding = this.xAxisLength * (1 - 2 * this.paddingFraction);
        this.xAxisOffset = - this.xAxisLength * this.paddingFraction + this.xMin * this.xLengthMinusPadding / (this.xMax - this.xMin);
        this.xDrawnMin = this.xAxisOffset + this.xAxisLength * paddingFraction;

        this.axesHandler.xAxisRod.transform.localScale = new Vector3 (this.xAxisLength, this.axesHandler.xAxisRod.transform.localScale.y, this.axesHandler.xAxisRod.transform.localScale.z);
        if (this.xAxisLabelPos == "along") {
            this.axesHandler.xAxisLabel.transform.localPosition = new Vector3 (this.xAxisLength / 2 + this.xAxisOffset, -2 * this.ticLabelDistanceVertical, 0f);
        }
        else if (this.xAxisLabelPos == "end") {
            this.axesHandler.xAxisLabel.transform.localPosition = new Vector3 (this.xAxisLength + this.xAxisOffset + this.ticLabelDistanceVertical * 1.1f, 0f, 0f);
        }
        //Axis arrowheads
        switch (this.arrows)
        {
            case "both":
                this.axesHandler.xArrows[0].transform.localPosition = new Vector3 (this.xAxisLength + this.xAxisOffset, 0f, 0f);
                this.axesHandler.xArrows[1].transform.localPosition = new Vector3 (this.xAxisOffset, 0f, 0f);
                break;
            case "positive":
                this.axesHandler.xArrows[0].transform.localPosition = new Vector3 (this.xAxisLength + this.xAxisOffset, 0f, 0f);
                break;
            case "neither":
                break;
            default:
                Debug.LogWarning("Graph arrow setting undefined. Defaulting to none.");
                break;
        }
    }
    private void updateXRangeProperties() {
        foreach (Tic tic in axesHandler.xTics) {
            tic.SetPosition();
        }
        axesHandler.HandleTicsX();
        if (labelsFaceCamera) {
            UpdateLabelList();//Definitely a more efficient way to do this, but maybe it doesn't matter.
        }
        pointData.RefreshData();
        foreach(var id in curves.Keys) {
            curves[id].RefreshData();
        }
        foreach (var id in surfaces.Keys) {
            surfaces[id].RefreshData();
        }
        foreach (var id in stackedAreas.Keys) {
            var s = stackedAreas[id];
            s.ScaleMesh();
        }
        if (barData != null) {
            barData.AdjustBarSpaceScale();
        }
    }
    public Vector3 CoordinateToPosition(Vector3 graphSpaceCoords) {
        Vector3 newPosOfChild = new Vector3 (
            graphSpaceCoords.x * xLengthMinusPadding / (xMax - xMin),
            graphSpaceCoords.y * yLengthMinusPadding / (yMax - yMin),
            graphSpaceCoords.z * zLengthMinusPadding / (zMax - zMin)
        );
        return newPosOfChild;
    }
    public Vector3 CoordinateToPosition(float x, float y, float z) {
        return CoordinateToPosition(new Vector3(x, y, z));
    }

    public void SetLabelFacing(bool facing = true) {
        labelsFaceCamera = facing;
        if (facing) {
            UpdateLabelList();
        }
    }
    private void UpdateLabelList() {
        allLabels = new List<PrimerObject>();

        allLabels.AddRange(axesHandler.xTics);
        allLabels.AddRange(axesHandler.yTics);
        allLabels.AddRange(axesHandler.zTics);
        allLabels.Add(axesHandler.xAxisLabel);
        allLabels.Add(axesHandler.yAxisLabel);
        allLabels.Add(axesHandler.zAxisLabel);
    }
    internal void AutoTicStep(bool x = false, bool y = false, bool z = false) {
        if (x) {
            while (xMax / xTicStep > 6) {
                if (xTicStep.ToString()[0] == '1') {
                    xTicStep *= 2;
                }
                else if (xTicStep.ToString()[0] == '2') {
                    xTicStep = Mathf.RoundToInt(xTicStep * 2.5f);
                }
                else if (xTicStep.ToString()[0] == '5') {
                    xTicStep *= 2;
                }
            }
        }
        if (y) {
            while (yMax / yTicStep > 6) {
                if (yTicStep.ToString()[0] == '1') {
                    yTicStep *= 2;
                }
                else if (yTicStep.ToString()[0] == '2') {
                    yTicStep = Mathf.RoundToInt(yTicStep * 2.5f);
                }
                else if (yTicStep.ToString()[0] == '5') {
                    yTicStep *= 2;
                }
            }
        }
        if (z) {
            while (zMax / zTicStep > 6) {
                if (zTicStep.ToString()[0] == '1') {
                    zTicStep *= 2;
                }
                else if (zTicStep.ToString()[0] == '2') {
                    zTicStep = Mathf.RoundToInt(zTicStep * 2.5f);
                }
                else if (zTicStep.ToString()[0] == '5') {
                    zTicStep *= 2;
                }
            }
        }
    }
    internal void AddLine() {
        
    }
    protected override IEnumerator scaleTo(Vector3 newScale, float duration, EaseMode ease) {
        Vector3 initialScale = transform.localScale;
        
        //Assume scale uniform, which should always be the case for graphs
        float scaleFactor = (float) newScale.x / initialScale.x;
        Dictionary<string, float> initialWidths = new Dictionary<string, float>();
        foreach (KeyValuePair<string, CurveData> entry in curves) {
            //Assumes all linerenderers in a curve are the same width
            initialWidths.Add(entry.Key, entry.Value.lineRenderers[0].widthMultiplier);
        }
        
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            transform.localScale = Vector3.Lerp(initialScale, newScale, t);
            foreach (KeyValuePair<string, CurveData> entry in curves) {
                foreach (LineRenderer lr in entry.Value.lineRenderers) {
                    lr.widthMultiplier = Mathf.Lerp(initialWidths[entry.Key], initialWidths[entry.Key] * scaleFactor, t);
                }
            }
            yield return null;
        }

        transform.localScale = newScale; //Ensure we actually get exactly to newScale 
    }
}
