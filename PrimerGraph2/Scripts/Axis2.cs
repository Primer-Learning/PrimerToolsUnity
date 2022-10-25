using System;
using System.Collections.Generic;
using System.Linq;
using Primer;
using PrimerGraph;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class Axis2 : ObjectGenerator
{
    // Configuration values
    public bool hidden;
    public bool flip;
    public string label = "Label";
    public AxisLabelPosition labelPosition = AxisLabelPosition.End;
    public ArrowPresence arrowPresence = ArrowPresence.Both;
    [Min(0.1f)] public float length = 1;
    [FormerlySerializedAs("thinkness")]
    public float thickness = 1;
    public float min;
    public float max = 10;


    [Header("Tics")]
    public bool showTics = true;
    [Min(0)] public float ticStep = 2;
    [Tooltip("Ensures no more ticks are rendered.")]
    [Range(1, 100)] public int maxTics = 50;
    public List<TicData> manualTics = new List<TicData>();

    [Header("Required elements")]
    public Transform rod;

    // Graph accessors
    Graph2 graph;
    PrimerText2 primerTextPrefab => graph.primerTextPrefab;
    PrimerBehaviour arrowPrefab => graph.arrowPrefab;
    Tic2 ticPrefab => graph.ticPrefab;
    float paddingFraction => graph.paddingFraction;
    float ticLabelDistance => graph.ticLabelDistance;

    // Internal game object containers
    readonly List<Tic2> tics = new List<Tic2>();
    PrimerText2 axisLabel;
    PrimerBehaviour originArrow;
    PrimerBehaviour endArrow;

    // Calculated fields
    float positionMultiplier => length * (1 - 2 * paddingFraction) / (max - min);
    float offset => -length * paddingFraction + min * positionMultiplier;

    // Memory
    ArrowPresence lastArrowPresence = ArrowPresence.Neither;

    void Awake() {
        graph = transform.parent.GetComponent<Graph2>();

        if (!graph) {
            // graph = transform.parent.gameObject.AddComponent<Graph2>();
            Debug.LogError("Axis component requires Graph to be present in parent");
            throw new Exception("Axis component requires Graph to be present in parent");
        }
    }


    public override void UpdateChildren() {
        gameObject.SetActive(!hidden);

        if (hidden) {
            RemoveGeneratedChildren();
            return;
        }

        UpdateRod();
        UpdateLabel();
        UpdateArrowHeads();
        UpdateTics();
    }

    protected override void OnChildrenRemoved() {
        tics.Clear();
        axisLabel = null;
        originArrow = null;
        endArrow = null;
        lastArrowPresence = ArrowPresence.Neither;
    }

    void UpdateRod() {
        rod.transform.localPosition = new Vector3(offset, 0f, 0f);
        rod.transform.localScale = new Vector3(length, thickness, thickness);
    }

    void UpdateLabel() {
        if (!axisLabel) {
            axisLabel = Create(primerTextPrefab);
            axisLabel.invertRotation = transform;
            axisLabel.ScaleUpFromZero();
        }

        var labelPos = Vector3.zero;

        if (labelPosition == AxisLabelPosition.Along) {
            labelPos = new Vector3(length / 2 + offset, -2 * ticLabelDistance, 0f);
        }
        else if (labelPosition == AxisLabelPosition.End) {
            labelPos = new Vector3(length + offset + ticLabelDistance * 1.1f, 0f, 0f);
        }

        axisLabel.transform.localPosition = labelPos;
        axisLabel.text = label;
        axisLabel.alignment = TextAlignmentOptions.Midline;
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
            endArrow = Create(arrowPrefab, Quaternion.Euler(0f, 90f, 0f));
            endArrow.ScaleUpFromZero();
        }

        if (arrowPresence == ArrowPresence.Positive) {
            originArrow?.ShrinkAndDispose();
            originArrow = null;
            return;
        }

        if (!originArrow) {
            originArrow = Create(arrowPrefab, Quaternion.Euler(0f, -90f, 0f));
            originArrow.ScaleUpFromZero();
        }
    }

    void UpdateTics() {
        var showTics = this.showTics && ticStep > 0;

        if (!showTics) {
            foreach (var tic in tics) {
                tic.ShrinkAndDispose();
            }

            tics.Clear();
            return;
        }

        var expectedTics = manualTics.Count != 0 ? manualTics : CalculateTics();

        if (maxTics != 0 && expectedTics.Count() > maxTics) {
            // TODO: reduce amount of tics in a smart way
            expectedTics = expectedTics.Take(maxTics);
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
            var newTic = Create(ticPrefab);
            newTic.Initialize(primerTextPrefab, data, ticLabelDistance);

            // this weird assignation discards the asynchronous task
            // so the execution continues without waiting for the animation to finish
            // a warning would be shown in Unity if we remove it
            newTic.ScaleUpFromZero();

            tics.Add(newTic);
        }

        foreach (var tic in tics) {
            tic.transform.localPosition = new Vector3(tic.value * positionMultiplier, 0, 0);
        }
    }

    IEnumerable<TicData> CalculateTics() {
        var calculated = new List<TicData>();
        var step = (float)Math.Round(ticStep, 2);

        if (step <= 0) {
            return calculated;
        }

        for (var i = step; i < max; i += step)
            calculated.Add(new TicData(i, i.ToString()));

        for (var i = -step; i > min; i -= step)
            calculated.Add(new TicData(i, i.ToString()));

        return calculated;
    }
}
