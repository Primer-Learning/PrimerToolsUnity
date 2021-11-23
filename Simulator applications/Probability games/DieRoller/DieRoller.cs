using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
public class DieRoller : PrimerObject
{
    internal RollerGroup group = null;
    internal GameObject diePrefab = null;
    internal Vector3 trayPosition = Vector3.zero;
    internal Quaternion trayRotation = Quaternion.identity;
    internal Vector3 sourcePosition = new Vector3(-6.6f, 7.3f, 6.6f);
    internal Quaternion sourceRotation = Quaternion.identity;
    private Vector3 _rollDir = new Vector3(1, -0.4f, -1.4f).normalized;
    public Vector3 RollDir {
        get {
            return _rollDir;
        }
        set {
            value.Normalize();
            _rollDir = value;
        }
    }
    internal Vector3 sourceDisp = Vector3.zero;
    internal PrimerObject tray = null;
    internal PrimerObject source = null;  
    public delegate void NoneToVoidDelType();
    public NoneToVoidDelType onRoll;
    // public NoneToVoidDelType onRoll;


    internal float rollDelay = 0;
    
    PrimerObject currentDie;
    // Vector3 initialVelRange = new Vector3(10, -10, 10);
    // Vector3 dropCenter = new Vector3(0, 50, 0);
    internal int numFaces = 6;
    internal float stoppedThreshold = 0.01f;
    internal float landCheckPause = 0.7f;
    internal float maxTime = 3;

    internal float initialVel = 50;

    // internal List<int> results = new List<int>();

    internal bool currentlyRolling = false;
    bool ready = false;

    void NoneToVoidDefaultDelegate(){}

    protected override void Awake() {
        onRoll = NoneToVoidDefaultDelegate;
    }
    void Update() {
        if (group.StillRolling() && ready && !currentlyRolling) {
            Roll();
        }
    }
    
    internal void AnimateIn(float duration = 1) {
        StartCoroutine(animateIn(duration));
    }
    IEnumerator animateIn(float duration) {
        tray.SetIntrinsicScale();
        source.SetIntrinsicScale();
        source.transform.localScale = Vector3.zero;
        tray.ScaleUpFromZero(duration: duration / 2);
        yield return new WaitForSeconds(duration / 2);
        source.ScaleUpFromZero(duration: duration / 2);

        yield return new WaitForSeconds(duration);
        ready = true;
    }

    internal void Roll() {
        StartCoroutine(repeatedRolls());
    }
    IEnumerator repeatedRolls() {
        while (currentlyRolling) { yield return null; }
        currentlyRolling = true;
        while (group.StillRolling())
        {
            group.rollsSoFar++;
            if (currentDie != null) { currentDie.Disappear(duration: rollDelay, toPool: true); }
            onRoll();
            SpawnDie();
            yield return new WaitForSeconds(rollDelay);
            TossDie();
            yield return WaitUntilDieIsStopped();
            if (RecordResult() == 0) {
                group.rollsSoFar -= 1;
            }
        }
        currentlyRolling = false;
    }
    internal IEnumerator singleRoll() {
        while (currentlyRolling || !ready) { yield return null; }
        currentlyRolling = true;
        if (currentDie != null) { currentDie.Disappear(duration: rollDelay, toPool: true); }
        onRoll();
        SpawnDie();
        yield return new WaitForSeconds(rollDelay);
        TossDie();
        yield return WaitUntilDieIsStopped();
        if (RecordResult() == 0) {
            StartCoroutine(repeatedRolls());
        }
        currentlyRolling = false;
    }
    void SpawnDie() {
        if (currentDie != null) { currentDie.Disappear(duration: rollDelay, toPool: true); }
        currentDie = group.GetPooledObject().MakePrimerObject();
        if (group.colorList != null) {
            Color rColor = group.colorList[Director.sceneRandom2.Next(group.colorList.Count)];
            currentDie.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = rColor;
        }
        currentDie.GetComponent<Rigidbody>().velocity = Vector3.zero;
        currentDie.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        currentDie.GetComponent<Rigidbody>().isKinematic = true; 
        currentDie.transform.parent = transform;
        currentDie.transform.localScale = Vector3.one;
        currentDie.transform.localPosition = source.transform.localPosition + sourceDisp;
        currentDie.transform.parent = source.transform.FindDeepChild("bone_neck");
        currentDie.SetIntrinsicScale();
        // Hax to cast the c# random object state to a Unity random object which can generate random rotations
        Random.InitState(Director.sceneRandom.Next());
        currentDie.transform.localRotation = Random.rotationUniform;
        currentDie.ScaleUpFromZero();
    }
    void TossDie() {
        currentDie.transform.parent = transform;
        currentDie.GetComponent<Rigidbody>().isKinematic = false; 
        currentDie.GetComponent<Rigidbody>().velocity = initialVel * RollDir * currentDie.transform.lossyScale.x;
    }
    IEnumerator WaitUntilDieIsStopped() {
        Rigidbody die = currentDie.GetComponent<Rigidbody>();
        // Check that it's still a small delay apart
        // Or just quit if it has been too long. Idk how to reliably have it settle to zero.
        float startTime = Time.time;
        int stateIndex = 0;
        while (stateIndex < 2) {
            if (Time.time > startTime + maxTime) {
                Debug.Log("Roll timeout");
                break;
            }
            if (stateIndex > 0) {
                yield return new WaitForSeconds(landCheckPause);
            }
            if (die.velocity.sqrMagnitude < stoppedThreshold && die.angularVelocity.sqrMagnitude < stoppedThreshold) {
                stateIndex++;
            }
            else { stateIndex = 0; }
            yield return null;
        }
    }
    int RecordResult() {
        //Jank time
        int result = 0;
        if (Vector3.Dot(currentDie.transform.up, Vector3.up) > 0.99f) { result = 1; }
        else if (Vector3.Dot(currentDie.transform.right, Vector3.up) > 0.99f) { result = 2; }
        else if (Vector3.Dot(currentDie.transform.forward, Vector3.up) > 0.99f) { result = 4; }
        else if (Vector3.Dot(currentDie.transform.up, Vector3.up) < -0.99f) { result = 6; }
        else if (Vector3.Dot(currentDie.transform.right, Vector3.up) < -0.99f) { result = 5; }
        else if (Vector3.Dot(currentDie.transform.forward, Vector3.up) < -0.99f) { result = 3; }
        else { result = 0; }

        group.RecordResult(result);
        return result;
    }
}      

