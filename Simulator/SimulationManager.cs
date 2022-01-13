using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    internal static SimulationManager instance;
    internal int? simSeed;
    internal List<Simulator> sims = new List<Simulator>();

    internal Graph graph = null;
    public GameObject objectToPool;
    internal int numToPool = 100; //Override in subclass
    public List<GameObject> pooledObjects;
    public PrimerObject sunLight = null;
    internal bool sunCycling = false;
    protected virtual void Awake() {
        if (instance == null) {
            instance = this;
        }
        else {
            Destroy(this.gameObject);
        }
        if (objectToPool != null) {
            pooledObjects = new List<GameObject>();
            for (int i = 0; i < numToPool; i++) {
                GameObject go = (GameObject)Instantiate(objectToPool);
                go.transform.parent = transform;
                go.SetActive(false);
                pooledObjects.Add(go);
            }
        }
        else {
            Debug.LogWarning("No pooled object defined.");
        }
    }
    void Start() {
        
        if (sunLight == null) {
            sunLight = GameObject.Find("Directional Light").AddComponent<PrimerObject>();
            if (sunLight == null) {
                Debug.LogWarning("Cannot find light");
            }
        }
    }
    void StartSimulations() {
        foreach (Simulator sim in sims) {
            sim.Go();
        }
    }

    // Pooling
    internal GameObject GetPooledObject() {
        for (int i = 0; i < pooledObjects.Count; i++) {
            if (!pooledObjects[i].activeInHierarchy) {
                pooledObjects[i].SetActive(true);
                return pooledObjects[i];
            }
        }
        // Debug.LogWarning("Ran out of pooled objects, adding another to pool.");
        GameObject go = (GameObject)Instantiate(objectToPool);
        go.transform.parent = transform;
        go.SetActive(true);
        pooledObjects.Add(go);
        return go;
    }
    //Conditional IEnumerators so we can skip animations during testing
    public Coroutine StartConditionalCoroutine(IEnumerator aCoroutine) {
        return StartCoroutine(ConditionalIEnumeratorWrapper(aCoroutine));
    }
    public IEnumerator ConditionalIEnumeratorWrapper(IEnumerator aCoroutine) {
        while (aCoroutine.MoveNext()) {
            var c = aCoroutine.Current;
            var conditional = c as ConditionalYield;
            if (conditional != null && SceneManager.instance is Director && ((Director)SceneManager.instance).animating) {
                yield return conditional.yieldValue;
            }
            else if (conditional == null) {
                yield return c;
            }
        }
    }
    public class ConditionalYield {
        public object yieldValue;
        public ConditionalYield(object aYieldValue) {
            yieldValue = aYieldValue;
        }
    }

    // Basic setup
    internal virtual void SetUpGraph(Graph g) {
        if (graph != null) {Debug.LogWarning("Sim graph already set");}
        graph = g;
    }
    internal void SetAndSaveSeed(int newSeed) {
        // This is the main one
        Director.sceneRandom = new System.Random(newSeed);
        // This is for purely visual stuff so messing with that doesn't affect rng state
        Director.sceneRandom2 = new System.Random(newSeed);
        simSeed = newSeed;
        Debug.Log($"Seed: {simSeed}");
    }

    // Other methods needed in many sims
    internal void CycleSun(float duration = 1) {
        StartCoroutine(cycleSun(duration));
    }
    private IEnumerator cycleSun(float duration) {
        if (sunLight != null) {
            sunLight.RotateByEuler(new Vector3(0, 0, -360), duration: duration);
            sunLight.AnimateLightIntensityTo(0, duration: duration / 2);
            yield return new WaitForSeconds(duration / 2);
            sunLight.AnimateLightIntensityTo(1, duration: duration / 2);
        }
    }
}
public static class TransformFindSimulatorExtension
{
    public static Simulator FindSimulatorInAncestors(this Transform t)
    {
        Simulator sim = t.gameObject.GetComponent<Simulator>();
        if (sim != null) {
            return sim;
        }
        if (t.parent != null) {
            sim = t.parent.FindSimulatorInAncestors();
        }
        else {
            Debug.LogWarning("Simulator not found in ancestors");
        }
        return sim;
    }    
}
