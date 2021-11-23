using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The controller for all scatter plot data in a <see cref="Graph"/>
/// </summary>
public class PointData : MonoBehaviour
{
    #region variables
    public Graph plot;
    public PointController pointPrefab;
    public GameObject pointObjectPrefab; //Sphere by default, but shapes are fun
    public float pointObjectScale = 1f;
    
    internal Dictionary<string, PointController> data = new Dictionary<string, PointController>();
    private Stack<PointController> pooledObjects = new Stack<PointController>();
    #endregion
    
    #region public methods
    /// <summary>
    /// Adds a value to the scatter plot
    /// </summary>
    /// <param name="value">The value of the point, in the axis-space of the graph</param>
    /// <param name="id">Optional. A unique identifier for this point, i.e. "single agent demo"</param>
    /// <returns>The id specified, or if none, a GUID for the point</returns>
    public PointController AddPoint(Vector3 value, string id = "", float duration = 0.5f, float scale = 0) {
        AdjustPointSpaceScale();
        if (id == "") id = System.Guid.NewGuid().ToString();
        var p = getAvailablePoint();
        p.MovePoint(value);
        if (scale == 0) {
            scale = pointObjectScale;
        }
        if (p.pointObject == null) {
            p.pointObject = Instantiate(pointObjectPrefab, p.transform);
            p.pointObject.transform.localScale = new Vector3(scale, scale, scale);
        }
        p.ActivatePoint(duration);
        data.Add(id, p);
        return p;
        //return id;
    }

    public void AdjustPointSpaceScale() {
        float xScale = plot.xLengthMinusPadding / (plot.xMax - plot.xMin);        
        float yScale = plot.yLengthMinusPadding / (plot.yMax - plot.yMin);        
        float zScale = plot.zLengthMinusPadding / (plot.zMax - plot.zMin);        
        if (plot.rightHanded) { zScale *= -1; }
        transform.localScale = new Vector3 (xScale, yScale, zScale);
    }

    /// <summary>
    /// Removes a point with a given id
    /// </summary>
    /// <param name="id">id to look for</param>
    /// <returns>True if the point was successfully removed, false if not found</returns>
    public bool RemovePoint(string id, float duration = 0.5f) {
        if (data.ContainsKey(id)) {
            var p = data[id];
            data.Remove(id);
            pooledObjects.Push(p);
            p.DeactivatePoint(duration);
            return true;
        }
        return false;
    } 

    /// <summary>
    /// Get the point with a given id
    /// </summary>
    /// <param name="id">The id of the point</param>
    /// <returns>The PointController with a given id. If none exists, returns null</returns>
    public PointController GetPoint(string id) {
        if (data.ContainsKey(id)) {
            return data[id];
        }
        return null;
    }
    #endregion

    #region internal methods
    internal void RefreshData() {
        transform.localRotation = Quaternion.identity; //Really shouldn't be rotated relative to parent
        transform.localPosition = Vector3.zero; //Shouldn't be dispalced relative to parent
        AdjustPointSpaceScale();
        var destroyed = new List<string>();
        foreach (var k in data.Keys) {
            if (data[k] == null) destroyed.Add(k);
            else {
                data[k].Refresh();
            }
        }
        foreach (var k in destroyed) {
            data.Remove(k);
        }
    }
    #endregion
    #region private methods
    private PointController getAvailablePoint() {
        var p = Instantiate(pointPrefab, transform);
        p.transform.localScale = Vector3.zero; //Assumes all points are scaled up from zero
        p.plot = plot;
        return p;
    }
    #endregion
}
