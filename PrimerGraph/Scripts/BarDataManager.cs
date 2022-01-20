using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A 3D surface of a <see cref="Graph"/>
/// </summary>
public class BarDataManager: MonoBehaviour
{
    #region variables
    public Graph plot;
    [SerializeField] PrimerObject barPrefab = null;

    internal List<float> values = new List<float>();
    private List<PrimerObject> bars = new List<PrimerObject>();
    // Bar appearance
    List<Color> barColors = new List<Color>();
    internal float barWidth = 1;
    
    // private bool animationDone = true;

    #endregion

    void Awake() {
        // plot = transform.parent.gameObject.GetComponent<Graph>();
        bars = new List<PrimerObject>();
    }
    private void GenerateBars(int totalBars = 0) {
        if (totalBars == 0) { totalBars = values.Count; }
        // Just makes sure there are enough
        for (int i = 0; i < totalBars; i++)
        {
            if (i == bars.Count) {
                PrimerObject newBar = Instantiate(barPrefab);
                newBar.transform.parent = transform;
                newBar.transform.localPosition = new Vector3(i + 1, 0, 0);
                newBar.transform.localRotation = Quaternion.Euler(0, 0, 0);
                newBar.transform.localScale = new Vector3(barWidth, 0, 1);
                bars.Add(newBar);
            }
            else if (i > bars.Count) {
                Debug.LogError("Bar generation is effed.");
            }
        }
        SetColors();
    }

    internal void AnimateBars(List<float> newVals, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        StartCoroutine(animateBars(newVals, duration, ease));
    }
    internal void AnimateBars(List<int> newVals, float duration = 0.5f, EaseMode ease = EaseMode.Cubic) {
        AnimateBars(IntListToFloat(newVals), duration, ease);
    }
    IEnumerator animateBars(List<float> newVals, float duration, EaseMode ease) {
        GenerateBars(newVals.Count);
        List<float> oldVals = values;
        float startTime = Time.time;
        while (Time.time < startTime + duration) {
            float t = (Time.time - startTime) / duration;
            t = Helpers.ApplyNormalizedEasing(t, ease);
            for (int i = 0; i < newVals.Count; i++)
            {
                if (i == oldVals.Count) {
                    oldVals.Add(0);
                }
                values[i] = Mathf.Lerp(oldVals[i], newVals[i], t);    
            }
            UpdateBars(values);
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < newVals.Count; i++)
        {
            values[i] = newVals[i];
        }
        UpdateBars(values);
    }

    internal void UpdateBars(List<float> newVals) {
        values = newVals;
        for (int i = 0; i < values.Count; i++)
        {
            bars[i].transform.localScale = new Vector3(barWidth, values[i], 1);
        }
    }
    internal void UpdateBars(List<int> newVals) {
        UpdateBars(IntListToFloat(newVals));
    }
    private List<float> IntListToFloat(List<int> ints) {
        List<float> fvals = new List<float>();
        foreach (int val in ints) {
            fvals.Add(val);
        }
        return fvals;
    }

    internal void SetColors(List<Color> colors) {
        barColors = colors;
        SetColors();
    }
    void SetColors() {
        for (int i = 0; i < bars.Count; i++) {
            Color c = barColors[i % barColors.Count];
            bars[i].SetEmissionColor(c);
        }
    }

    internal void RefreshData() {
        transform.localRotation = Quaternion.identity; //Really shouldn't be rotated relative to parent
        transform.localPosition = Vector3.zero; //Shouldn't be displaced relative to parent
        AdjustBarSpaceScale();
        GenerateBars();
        UpdateBars(values);
    }
    internal void AdjustBarSpaceScale() {
        float xScale = plot.xLengthMinusPadding / (plot.xMax - plot.xMin);        
        float yScale = plot.yLengthMinusPadding / (plot.yMax - plot.yMin);        
        transform.localScale = new Vector3 (xScale, yScale, 1);
    }
    private void OnApplicationQuit() {
        ForceStopAnimation();
    }

    private void ForceStopAnimation() {
        StopAllCoroutines();
    }
    internal static Dictionary<float, string> GenerateIntegerCategories(int num, int min = 0, int step = 1) {
        Dictionary<float, string> categoryDict = new Dictionary<float, string>();
        for (int i = 0; i < num; i++) {
            categoryDict.Add(1 + i, (min + i * step).ToString());
        };
        return categoryDict;
    }
}
