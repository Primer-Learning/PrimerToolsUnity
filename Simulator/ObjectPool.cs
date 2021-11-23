using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    // SimulationManager already handles pooling. This is old, but it might be useful if all you want is pooling.
    public GameObject objectToPool;
    internal int numToPool = 100; 
    public List<GameObject> pooledObjects;

    void Start() {
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
    internal GameObject GetPooledObject() {
        for (int i = 0; i < pooledObjects.Count; i++) {
            if (!pooledObjects[i].activeInHierarchy) {
                pooledObjects[i].SetActive(true);
                return pooledObjects[i];
            }
        }
        Debug.LogError("Ran out of pooled objects");
        return null;
    }
}
