using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 3D surface of a <see cref="Graph"/>
/// </summary>
public class SurfaceData : MonoBehaviour
{
    #region serialized fields
    public MeshFilter planeMeshFilter;
    public MeshRenderer planeRenderer;
    #endregion

    #region variables
    internal Graph plot;
    internal Vector3 mins; //Min values for each graph axis
    internal Vector3 maxs; //Max values for each graph axis
    
    private Vector3[] dataPoints;
    private Vector3[] verts;
    private bool animationDone = true;

    Func<float, float, float> currentSurfaceFunction = null;
    #endregion

    #region public methods
    /// <summary>
    /// Sets the function of the surface
    /// </summary>
    /// <param name="f">The function of the surface, as y(x,z)</param>
    /// <param name="fNormal">Optional. The normal direction of the surface (only needed if you want to start using lighting on the mesh. Avoid if possible</param>
    public void SetFunction(Func<float, float, float> f, Func<float, float, float, Vector3> fNormal = null) {
        transform.localRotation = Quaternion.identity;
        if (plot.rightHanded) {
            transform.localScale = new Vector3 (1, 1, -1);
        }
        transform.localPosition = Vector3.zero;
        currentSurfaceFunction = f;
        makeGraphSpaceCoords();
        RefreshData();
    }

    /// <summary>
    /// Change the color of the surface
    /// </summary>
    /// <param name="startYColor">The color at the min Y value</param>
    /// <param name="endYColor">The color at the max Y value</param>
    public void UpdateColors(Color startYColor, Color endYColor) {
        planeRenderer.material.SetColor("_StartColor", startYColor);
        planeRenderer.material.SetColor("_EndColor", endYColor);
    }

    /// <summary>
    /// Sets the visible range of the surface in the x,y,z dimensions. Each value (x,y,z) should be [0,1].
    /// </summary>
    /// <param name="min">The lower bound of the visible range</param>
    /// <param name="max">The upper bound of the viblible range</param>
    public void UpdateVisibleRange(Vector3 min, Vector3 max) {
        planeRenderer.material.SetVector("_VisibleMin", min);
        planeRenderer.material.SetVector("_VisibleMax", max);
    }

    public void UpdateRange(Vector3 min, Vector3 max) {
        planeRenderer.material.SetVector("_Min", min);
        planeRenderer.material.SetVector("_Max", max);
    }

    /// <summary>
    /// Updates the scaling of the texture on the surface
    /// </summary>
    /// <param name="size">The size of the square</param>
    public void UpdateTexture(float size) {
        var scale = new Vector2(size * transform.lossyScale.x, size * transform.lossyScale.z);
        planeRenderer.material.mainTextureScale = scale;
        planeRenderer.material.mainTextureOffset = new Vector2((int)scale.x - scale.x, (int)scale.y - scale.y);
    }

    /// <summary>
    /// Morph from one surface to another (along the y axis)
    /// </summary>
    /// <param name="duration">The duration of the sweep in seconds</param>
    public void AnimateToNewSurface(Func<float, float, float> to, float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            if (animationDone) {
                if (currentSurfaceFunction == null) {
                    SetFunction(to);
                    AnimateX(duration, ease);
                }
                else if (to == null) {
                    SetFunction(to);
                    RefreshData();
                }
                else {
                    StartCoroutine(animateSurfaceTransition(currentSurfaceFunction, to, duration, ease));
                }
            }
            else {
                ForceStopAnimation();
            }
        }
        else {
            SetFunction(to);
        }

    }

    /// <summary>
    /// Sweeps in the curve along the X axis
    /// </summary>
    /// <param name="duration">The duration of the sweep in seconds</param>
    public void AnimateX(float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            if (animationDone)
                StartCoroutine(animateVisibleRange(new Vector3(mins.x, maxs.y, maxs.z), maxs, duration, ease));
            else
                ForceStopAnimation();
        }
        else {
            UpdateVisibleRange(mins, maxs);
        }
    }

    /// <summary>
    /// Sweeps in the curve along the Y axis
    /// </summary>
    /// <param name="duration">The duration of the sweep in seconds</param>
    public void AnimateY(float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            if (animationDone)
                StartCoroutine(animateVisibleRange(new Vector3(maxs.x, mins.y, maxs.z), maxs, duration, ease));
            else
                ForceStopAnimation();
        }
        else {
            UpdateVisibleRange(mins, maxs);
        }
    }

    /// <summary>
    /// Sweeps in the curve along the Z axis
    /// </summary>
    /// <param name="duration">The duration of the sweep in seconds</param>
    public void AnimateZ(float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            if (animationDone)
                StartCoroutine(animateVisibleRange(new Vector3(maxs.x, maxs.y, mins.z), maxs, duration, ease));
            else
                ForceStopAnimation();
        }
        else {
            UpdateVisibleRange(mins, maxs);
        }
    }
    #endregion

    #region internal methods
    internal void RefreshData() {
        if (planeMeshFilter == null) {
            Debug.LogError("Mesh Filter Not defined");
            return;
        }
        
        var plane = planeMeshFilter.mesh;
        verts = new Vector3[plane.vertexCount];

        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float maxZ = -Mathf.Infinity;
        
        for (int i = 0; i < verts.Length; i++) {
            //Convert vertex from graph space to draw space
            verts[i].x = dataPoints[i].x * plot.xLengthMinusPadding / (plot.xMax - plot.xMin);
            verts[i].y = dataPoints[i].y * plot.yLengthMinusPadding / (plot.yMax - plot.yMin);
            verts[i].z = dataPoints[i].z * plot.zLengthMinusPadding / (plot.zMax - plot.zMin);

            //Assumes plane object's scale is 1
            minX = Mathf.Min(minX, verts[i].x);
            minY = Mathf.Min(minY, verts[i].y); 
            minZ = Mathf.Min(minZ, verts[i].z);
            maxX = Mathf.Max(maxX, verts[i].x);
            maxY = Mathf.Max(maxY, verts[i].y);
            maxZ = Mathf.Max(maxZ, verts[i].z);

            plane.vertices = verts;
            plane.RecalculateNormals();
            plane.RecalculateBounds();
        }
        UpdateRange(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ));
    }
    #endregion

    #region private methods
    void Start() {
        //UpdateRange(mins, maxs);
    }

    void makeGraphSpaceCoords() {
        Mesh plane = planeMeshFilter.mesh;
        var boundsMin = plane.bounds.center - plane.bounds.extents;
        var boundsMax = plane.bounds.center + plane.bounds.extents;

        //verts = new Vector3[plane.vertexCount];
        dataPoints = new Vector3[plane.vertexCount];

        for (int i = 0; i < dataPoints.Length; i++) {
            var v = plane.vertices[i];
                        
            //Define graph-space data points corresponding to mesh vertices
            dataPoints[i].x = Axes.MapFloat(v.x, boundsMin.x, boundsMax.x, plot.xMin, plot.xMax);
            dataPoints[i].z = Axes.MapFloat(v.z, boundsMin.z, boundsMax.z, plot.zMin, plot.zMax);
            dataPoints[i].y = currentSurfaceFunction(dataPoints[i].x, dataPoints[i].z);
        }
    }

    private IEnumerator animateVisibleRange(Vector3 from, Vector3 to, float duration, EaseMode ease) {
        animationDone = false;
        
        //int initFrame = Time.frameCount;
        //float durationInFrames = duration * Application.targetFrameRate;

        float startTime = Time.time;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            UpdateVisibleRange(mins, Vector3.Lerp(from, to, t));
            yield return new WaitForEndOfFrame();
        }
        UpdateVisibleRange(mins, to);
        animationDone = true;
    }
    public void WipeSurfaceX(float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(wipeVisibleRange(new Vector3(maxs.x, mins.y, mins.z), duration, ease));
    }
    private IEnumerator wipeVisibleRange(Vector3 final, float duration, EaseMode ease) {
        animationDone = false;
        
        float startTime = Time.time;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            //Assumes we're starting with whole min to max range showing
            UpdateVisibleRange(Vector3.Lerp(mins, final, t), maxs);
            yield return new WaitForEndOfFrame();
        }
        UpdateVisibleRange(final, maxs);
        animationDone = true;
    }

    private void OnApplicationQuit() {
        ForceStopAnimation();
    }

    private void ForceStopAnimation() {
        StopAllCoroutines();

        animationDone = true;
        UpdateVisibleRange(mins, maxs);
    }
    
    private IEnumerator animateSurfaceTransition(Func<float, float, float> from, Func<float, float, float> to, float duration, EaseMode ease) {
        animationDone = false;
        Func<float, float, float> lerpedFunc;

        float init = Time.time;

        while (Time.time - init < duration) {
            float t = (Time.time - init) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            lerpedFunc = (x, y) => Mathf.Lerp(from(x, y), to(x, y), t);
            SetFunction(lerpedFunc);
            yield return new WaitForEndOfFrame();
        }
        SetFunction(to);
        animationDone = true;
    }

    #endregion
}
