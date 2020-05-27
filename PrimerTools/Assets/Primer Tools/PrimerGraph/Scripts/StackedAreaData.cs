using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 3D surface of a <see cref="Graph"/>
/// </summary>
public class StackedAreaData : MonoBehaviour
{
    #region serialized fields
    public MeshFilter planeMeshFilter;
    public MeshRenderer planeRenderer;
    #endregion

    #region variables
    internal Graph plot;
    internal Vector3 mins; //Min values for each graph axis
    internal Vector3 maxs; //Max values for each graph axis
    
    private Vector3[] verts;
    private bool animationDone = true;

    private int numValuesInShaderFloatArray = 900; //Number of entries in shader float arrays. Big because you can't change their size.
    internal int pointsPerUnit = 1; //In case we want graph and function to have different units
    private float shaderXStep; //Use this to scale the mesh
    private int clippingNumber = 0;
    #endregion

    void Awake() {
        //Set default colors on awake
        //SetColors();
    }

    #region public methods
    /// <summary>
    /// Sets the function of the surface
    /// </summary>
    /// <param name="f">The function of the surface, as y(x,z)</param>
    /// <param name="fNormal">Optional. The normal direction of the surface (only needed if you want to start using lighting on the mesh. Avoid if possible</param>
    public void SetFunctions(List<float> func1, List<float> func2, List<float> func3, List<float> func4) {
        if (func1.Count != func2.Count || func1.Count != func3.Count) {
            Debug.LogError("Stacked area functions need to be the same length, but they aren't.");
        }
        setShaderFloatArray("_FuncValues1", func1);
        setShaderFloatArray("_FuncValues2", func2);
        setShaderFloatArray("_FuncValues3", func3);
        setShaderFloatArray("_FuncValues4", func4);
    }
    public void SetFunctions(List<float> func1, List<float> func2, List<float> func3) {
        if (func1.Count != func2.Count || func1.Count != func3.Count) {
            Debug.LogError("Stacked area functions need to be the same length, but they aren't.");
        }
        setShaderFloatArray("_FuncValues1", func1);
        setShaderFloatArray("_FuncValues2", func2);
        setShaderFloatArray("_FuncValues3", func3);
    }
    public void SetFunctions(List<float> func1, List<float> func2) {
        if (func1.Count != func2.Count) {
            Debug.LogError("Stacked area functions need to be the same length, but they aren't.");
        }
        setShaderFloatArray("_FuncValues1", func1);
        setShaderFloatArray("_FuncValues2", func2);
    }
    public void SetFunctions(List<float> func1) {
        setShaderFloatArray("_FuncValues1", func1);
    }
    private void setShaderFloatArray(string name, List<float> l) {
        List<float> finalList = new List<float>();
        for (int i = 0; i < numValuesInShaderFloatArray; i++) {
            if (i < l.Count) {
                finalList.Add(l[i]);
            }
            else {
                finalList.Add(0);
            }
        }
        Material mat = planeRenderer.material;
        mat.SetFloatArray(name, finalList.ToArray());
        //Only need to do this one time, or zero if it's 900 ¯\_(ツ)_/¯
        mat.SetInt("_ValuesCount", numValuesInShaderFloatArray);
    }

    public void SetColors(Color c1, Color c2, Color c3, Color c4) {
        Material mat = planeRenderer.material;
        mat.SetColor("_Color1", c1);
        mat.SetColor("_Color2", c2);
        mat.SetColor("_Color3", c3);
        mat.SetColor("_Color4", c4);
    }
    public void SetColors(Color c1, Color c2, Color c3) {
        Material mat = planeRenderer.material;
        mat.SetColor("_Color1", c1);
        mat.SetColor("_Color2", c2);
        mat.SetColor("_Color3", c3);
    }
    public void SetColors(Color c1, Color c2) {
        Material mat = planeRenderer.material;
        mat.SetColor("_Color1", c1);
        mat.SetColor("_Color2", c2);
    }
    public void SetColors(Color c1) {
        Material mat = planeRenderer.material;
        mat.SetColor("_Color1", c1);
    }

    /*
    public void SetColors(Color[] colors) {
        planeRenderer.material.SetColorArray("_Colors", colors);
    }
    public void SetColors() {
        Color[] colors = new Color[] {
            PrimerConstants.GetColor("Blue"),
            PrimerConstants.GetColor("Orange"),
            PrimerConstants.GetColor("Red"),
            PrimerConstants.GetColor("Green"),
            new Color(0, 0, 0, 0)
        };
        planeRenderer.material.SetFloatArray("_Colors", colors);
    }
    */

    /*
    public void UpdateFunction(float[] func) {
        planeRenderer.material.SetVector();
    }
    */

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
    /// Sweeps in the curve along the X axis
    /// </summary>
    /// <param name="duration">The duration of the sweep in seconds</param>
    public void AnimateX(float duration = 1.5f, EaseMode ease = EaseMode.Cubic) {
        if (Application.isPlaying) {
            if (animationDone) {
                planeRenderer.material.SetFloat("_ClippingNumber", numValuesInShaderFloatArray); 
                StartCoroutine(animateVisibleRange(new Vector3(mins.x, maxs.y, maxs.z), maxs, duration, ease));
            }
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
    #endregion

    #region internal methods
    internal void RefreshData() {
        if (planeMeshFilter == null) {
            Debug.LogError("Mesh Filter Not defined");
            return;
        }

        Mesh plane = planeMeshFilter.mesh;
        var boundsMin = plane.bounds.center - plane.bounds.extents;
        var boundsMax = plane.bounds.center + plane.bounds.extents;

        verts = new Vector3[plane.vertexCount];

        //The relationship between mesh vertices, the shader, and the mesh scale is the result of 
        //a changing plan. Probably quite confusing and worth considering a refactor before adding features.
        for (int i = 0; i < verts.Length; i++) {
            var v = plane.vertices[i];
            //Remap vertices to graph range because the shader uses the vertex coords
            verts[i].x = Axes.MapFloat(v.x, boundsMin.x, boundsMax.x, plot.xMin, plot.xMax);
            verts[i].y = Axes.MapFloat(v.z, boundsMin.z, boundsMax.z, plot.yMin, plot.yMax);
            verts[i].z = 0;
        }
        plane.vertices = verts;
        plane.RecalculateNormals();
        plane.RecalculateBounds();
        UpdateRange(mins, maxs);

        shaderXStep = numValuesInShaderFloatArray / (plot.xMax - plot.xMin);

        ScaleMesh();
    }
    public void ScaleMesh() {
        //Rescale plane to plot area (minus paddeing)
        //Divide by correct for the stretched out vertices above
        planeMeshFilter.gameObject.transform.localScale = new Vector3 (
            //Stretch along x in proportion to number of values in shader float arrays
            plot.xLengthMinusPadding / (plot.xMax - plot.xMin) * shaderXStep / pointsPerUnit,
            plot.yLengthMinusPadding / (plot.yMax - plot.yMin),
            1
        );
    }
    public void AnimateClippingNumber(int newClippingNumber, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(animateClippingNumber(newClippingNumber, duration, ease));
    }
    #endregion

    #region private methods

    private IEnumerator animateVisibleRange(Vector3 from, Vector3 to, float duration, EaseMode ease) {
        animationDone = false;

        to.x *= pointsPerUnit;
        
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
    //TODO: Get rid of _ClippingNumber. We already have the stap size and clipping number, so we could just
    //_VisibleRange.
    private IEnumerator animateClippingNumber(int newClippingNumber, float duration, EaseMode ease) {
        animationDone = false;
        float startTime = Time.time;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            planeRenderer.material.SetFloat("_ClippingNumber", Mathf.Lerp(clippingNumber, newClippingNumber, t));
            yield return new WaitForEndOfFrame();
        }
        planeRenderer.material.SetFloat("_ClippingNumber", newClippingNumber);
        clippingNumber = newClippingNumber;
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

    #endregion
}
