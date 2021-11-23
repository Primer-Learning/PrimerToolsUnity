using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
public class RollerGroup : SimulationManager
{
    internal UpdateMode updateMode = UpdateMode.EveryTime;
    internal float updateInterval = 0.1f;
    bool updating = false;
    // internal bool rolling = false;
    internal int rollsSoFar = 0;
    internal int rollCap = 0;
    internal List<DieRoller> rollers = new List<DieRoller>();

    // Roller defaults    
    public GameObject diePrefab = null;
    public GameObject sourcePrefab = null;
    public GameObject trayPrefab = null;
    internal Vector3 trayPosition = Vector3.zero;
    internal Quaternion trayRotation = Quaternion.identity;
    internal Vector3 sourcePosition = new Vector3(-6.6f, 5.25f, 6.6f);
    internal Vector3 sourceFacing = new Vector3(1, 0, -1);
    internal Vector3 sourceDisp = new Vector3(1.6f, 2.5f, -1.6f);
    internal Vector3 sourceScale = Vector3.one * 3;
    int numFaces = 6;
    internal float rollDelay = 0.5f;
    float stoppedThreshold = 0.01f;
    float landCheckPause = 0.7f;
    float maxTime = 3;
    float initialVel = 50;
    private Vector3 rollDir = new Vector3(1, -0.4f, -1.4f).normalized;
    public delegate void NoneToVoidDelType();
    public NoneToVoidDelType onUpdate;

    // Graph graph = null;
    internal List<int> results = new List<int>();
    internal List<Color> colorList = null;
    void NoneToVoidDefaultDelegate(){}
    protected override void Awake() {
        base.Awake();
        onUpdate = NoneToVoidDefaultDelegate;
        Reset();
    }

    internal DieRoller AddRoller() {
        GameObject go = new GameObject("Roller");
        DieRoller nr = go.AddComponent<DieRoller>();
        nr.group = this;
        rollers.Add(nr);
        nr.diePrefab = diePrefab;
        nr.source = Instantiate(sourcePrefab, nr.transform, false).MakePrimerObject();
        nr.source.transform.localPosition = sourcePosition;
        nr.source.transform.forward = sourceFacing;
        nr.source.transform.localScale = sourceScale;
        nr.sourceDisp = this.sourceDisp;
        nr.tray = Instantiate(trayPrefab, nr.transform, false).MakePrimerObject();
        nr.tray.transform.localPosition = this.trayPosition;
        nr.tray.transform.localRotation = this.trayRotation;

        nr.numFaces = this.numFaces;
        nr.rollDelay = this.rollDelay;
        nr.stoppedThreshold = this.stoppedThreshold;
        nr.landCheckPause = this.landCheckPause;
        nr.maxTime = this.maxTime;
        nr.initialVel = this.initialVel;
        nr.RollDir = this.rollDir;

        PrimerCharacter b = nr.source.GetComponent<PrimerCharacter>();
        if (b != null) {
            nr.onRoll = () => { b.animator.SetTrigger("Scoop"); };
        }

        return nr;
    }

    void Reset() {
        rollsSoFar = 0;
        rollCap = 0;
        results = new List<int>();
        for (int i = 0; i <= numFaces; i++)
        {
            results.Add(0);
        }
    }

    internal void Roll(int numRolls = 1, bool reset = true) {
        StartCoroutine(roll(numRolls, reset));
    }

    internal IEnumerator roll(int numRolls, bool reset = true) {
        //This is a coroutine so a scene can easily wait for it.
        if (reset) { Reset(); }
        rollCap += numRolls;
        if (updateMode == UpdateMode.Interval) {
            BeginUpdating();
        }
        while (rollsSoFar < rollCap) {
            yield return null;
        }
        while (RollersStillWorking()) {
            yield return null;
        }
    }

    internal bool StillRolling() {
        return (rollsSoFar < rollCap);
    }
    internal bool RollersStillWorking() {
        foreach (DieRoller r in rollers) {
            if (r.currentlyRolling) { return true; }
        }
        return false;
    }
    
    internal void BeginUpdating() {
        StartCoroutine(checkForUpdates());
    }
    IEnumerator checkForUpdates() {
        while (true) {
            yield return new WaitForSeconds(updateInterval);
            while (updating) { yield return null; }
            UpdateGraph();
        }
    }
    internal void RecordResult(int result) {
        results[result]++;
        // if (result > 0) { rollsSoFar++; }
        if (updateMode == UpdateMode.EveryTime) {
            UpdateGraph();
        }
    }

    void UpdateGraph(float duration = 0.5f) {
        updating = true;
        List<int> nonfails = results.Skip(1).ToList();
        // Update bars
        graph.barData.AnimateBars(nonfails, duration: duration);
        
        // Update range if necessary
        float graphYMax = nonfails.Max();
        if (graphYMax / graph.yTicStep > 6) {
            if (graph.yTicStep.ToString()[0] == '1') {
                graph.yTicStep *= 2;
            }
            else if (graph.yTicStep.ToString()[0] == '2') {
                graph.yTicStep = Mathf.RoundToInt(graph.yTicStep * 2.5f);
            }
            else if (graph.yTicStep.ToString()[0] == '5') {
                graph.yTicStep *= 2;
            }
        }
        if (graphYMax > graph.yMax) {
            graph.ChangeRangeY(graph.yMin, graphYMax, duration: duration);
        }
        onUpdate();
        StartCoroutine(noLongerUpdating(duration));
    }
    IEnumerator noLongerUpdating(float duration) {
        yield return new WaitForSeconds(duration);
        updating = false;
    }
    internal override void SetUpGraph(Graph graph) {
        base.SetUpGraph(graph);
        if (graph.barData == null) {
            graph.AddBarData();
        }
    }
}      

public enum UpdateMode {
    EveryTime,
    Interval
}