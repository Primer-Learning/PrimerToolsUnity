using System;
using System.Collections.Generic;
using PrimerBase;
using PrimerGraph;
using TMPro;
using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(Graph2))]
public class Axis2 : ObjectGenerator
{
    // Configuration values
    public bool hidden;
    public string label = "Label";
    [Min(0.1f)] public float length = 1;
    public float thinkness = 1;
    public float min;
    public float max = 10;
    public AxisLabelPosition labelPosition = AxisLabelPosition.End;
    public ArrowPresence arrowPresence = ArrowPresence.Both;
    // public float paddingFraction = 0.05f;

    [Header("Tics")]
    public bool showTics = true;
    [Min(0)] public float ticStep = 2;
    [Tooltip("Ensures no more ticks are rendered. Leave at 0 to have no limit. NOT IMPLEMENTED")]
    [Min(0)] public int maxTics = 5;
    public List<TicData> manualTics = new List<TicData>();

    [Header("Required elements")]
    public PrimerBehaviour container;
    public Transform rod;

    // Graph accessors
    Graph2 graph;
    PrimerText primerTextPrefab => graph.primerTextPrefab;
    PrimerBehaviour arrowPrefab => graph.arrowPrefab;
    Tic2 ticPrefab => graph.ticPrefab;
    float paddingFraction => graph.paddingFraction;
    float ticLabelDistance => graph.ticLabelDistanceVertical;

    // Internal game object containers
    readonly List<Tic2> tics = new List<Tic2>();
    PrimerText axisLabel;
    PrimerBehaviour originArrow;
    PrimerBehaviour endArrow;

    // Calculated fields
    float lengthMinusPadding => length * (1 - 2 * paddingFraction);
    float positionMultiplier => lengthMinusPadding / (max - min);
    float offset => -length * paddingFraction + min * positionMultiplier;

    // Memory
    ArrowPresence lastArrowPresence = ArrowPresence.Neither;

    void Awake() {
        Debug.Log("Awake");
        graph = GetComponent<Graph2>();
    }

    void Start() {
        Debug.Log("Start");
        RemoveGeneratedChildren();
    }

    void OnValidate() {
        generatedObjectsContainer = container?.transform;
    }

    void Update() {
        // Debug.Log("Update");
        Regenerate();
    }

    void OnDestroy() {
        Debug.Log("OnDestroy");
        RemoveGeneratedChildren();
    }

    public new void RemoveGeneratedChildren() {
        base.RemoveGeneratedChildren();
        tics.Clear();
        axisLabel = null;
        originArrow = null;
        endArrow = null;
        lastArrowPresence = ArrowPresence.Neither;
    }

    public void Regenerate() {
        container.gameObject.SetActive(!hidden);

        UpdateRod();
        UpdateLabel();
        UpdateArrowHeads();
        UpdateTics();
    }

    void UpdateRod() {
        rod.transform.localPosition = new Vector3(offset, 0f, 0f);
        rod.transform.localScale = new Vector3(length, thinkness, thinkness);
    }

    void UpdateLabel() {
        if (!axisLabel) {
            axisLabel = GenerateChild(primerTextPrefab);
            axisLabel.transform.localRotation = Quaternion.Inverse(container.transform.rotation);
        }

        var labelPos = Vector3.zero;

        if (labelPosition == AxisLabelPosition.Along) {
            labelPos = new Vector3(length / 2 + offset, -2 * ticLabelDistance, 0f);
        }
        else if (labelPosition == AxisLabelPosition.End) {
            labelPos = new Vector3(length + offset + ticLabelDistance * 1.1f, 0f, 0f);
        }

        axisLabel.transform.localPosition = labelPos;
        axisLabel.tmpro.text = label;
        axisLabel.tmpro.alignment = TextAlignmentOptions.Midline;
        axisLabel.SetIntrinsicScale();
    }

    void UpdateArrowHeads() {
        if (arrowPresence != lastArrowPresence) {
            lastArrowPresence = arrowPresence;
            RecreateArrowHeads();
        }

        if (endArrow) {
            endArrow.transform.localPosition = new Vector3(length + offset, 0f, 0f);
        }

        if (originArrow) {
            originArrow.transform.localPosition = new Vector3(offset, 0f, 0f);
        }
    }

    void RecreateArrowHeads() {
        if (arrowPresence == ArrowPresence.Neither) {
            endArrow?.ShrinkAndDispose();
            endArrow = null;
            originArrow?.ShrinkAndDispose();
            originArrow = null;
            return;
        }

        if (!endArrow) {
            endArrow = GenerateChild(arrowPrefab);
            endArrow.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }

        if (arrowPresence == ArrowPresence.Positive) {
            originArrow?.ShrinkAndDispose();
            originArrow = null;
            return;
        }

        if (!originArrow) {
            originArrow = GenerateChild(arrowPrefab);
            originArrow.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        }
    }

    void UpdateTics() {
        if (ticStep <= 0) return;

        if (!showTics) {
            foreach (var tic in tics) {
                tic.ShrinkAndDispose();
            }

            tics.Clear();
            return;
        }

        var expectedTics = manualTics.Count != 0 ? manualTics : CalculateTics();

        if (maxTics != 0 && expectedTics.Count > maxTics) {
            // TODO: reduce amount of tics in a smart way
        }

        var toAdd = new List<TicData>(expectedTics);
        var toRemove = new List<Tic2>(tics);

        // Reuse tics that exist in the right place
        foreach (var entry in expectedTics) {
            var found = toRemove.Find(tic => tic.value == entry.value);

            if (found) {
                found.label = entry.label;
                toRemove.Remove(found);
                toAdd.Remove(entry);
            }
        }

        foreach (var tic in toRemove) {
            tics.Remove(tic);
            tic.ShrinkAndDispose();
        }

        foreach (var data in toAdd) {
            var newTic = GenerateChild(ticPrefab);
            newTic.Initialize(primerTextPrefab, data, ticLabelDistance);

            // this weird assignation discards the asynchronous task
            // so the execution continues without waiting for the animation to finish
            // a warning would be shown in Unity if we remove it
            _ = newTic.ScaleUpFromZero();

            tics.Add(newTic);
        }

        foreach (var tic in tics) {
            tic.transform.localPosition = new Vector3(tic.value * positionMultiplier, 0, 0);
        }
    }

    List<TicData> CalculateTics() {
        var calculated = new List<TicData>();
        var step = (float)Math.Round(ticStep);

        for (var i = step; i < max; i += step)
            calculated.Add(new TicData(i, i.ToString()));

        for (var i = -step; i > min; i -= step)
            calculated.Add(new TicData(i, i.ToString()));

        return calculated;
    }
}
