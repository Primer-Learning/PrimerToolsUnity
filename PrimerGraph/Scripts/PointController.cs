using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single data point
/// </summary>
public class PointController : PrimerObject
{
    [HideInInspector] public Graph plot;
    private Vector3 value;
    public GameObject pointObject;

    #region public methods
    /// <summary>
    /// Moves point to new value in the graph
    /// </summary>
    /// <param name="to">The new value for the point</param>
    /// <param name="duration">How long (in seconds) it should take to move there</param>
    public void MovePoint(Vector3 to, float duration = 0.5f) {
        if(Application.isPlaying && transform.localScale != Vector3.zero) {
            this.MoveTo(to, duration);
            value = to;
        }
        else {
            value = to;
            Refresh();
        }
    }
    #endregion

    #region public methods
    public void Refresh() {
        transform.localPosition = value; 

        //Scale points to smallest axis, keeping them round
        float x = 1.0f / transform.parent.localScale.x;
        float y = 1.0f / transform.parent.localScale.y;
        float z = 1.0f / transform.parent.localScale.z;
        float s = Math.Max(x, Math.Max(y, z));
        transform.localScale = new Vector3(x / s, y / s, z / s);
        this.intrinsicScale = transform.localScale;
    }

    public void DeactivatePoint(float duration = 0.5f) {
        StartCoroutine(deactivatePoint(duration));
    }
    private IEnumerator deactivatePoint(float duration) {
        this.ScaleDownToZero(duration: duration);
        yield return new WaitForSeconds(duration);
        Destroy(this);
    }

    public void ActivatePoint(float duration) {
        this.ScaleUpFromZero(duration: duration);
    }
    #endregion

    #region private methods
    #endregion
}
