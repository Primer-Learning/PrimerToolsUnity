using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Primer
{
    public class ObjectManagementUtilities
    {
        // Gets or creates a root object with the given name.
        public static Transform GetRootTransform(string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : new GameObject(name);
            return obj.transform;
        }
        
        // Overload for the above that takes a prefab
        public static Transform GetRootTransform(GameObject prefab, string name)
        {
            var scene = SceneManager.GetActiveScene();
            var rootGameObjects = scene.GetRootGameObjects();
            var found = rootGameObjects.FirstOrDefault(x => x.name == name);
            var obj = found != null ? found : InstantiatePrefab(prefab);
            obj.name = name;
            return obj.transform;
        }

        // Gets or creates a child object with the given name.
        public static Transform GetDirectChild(Transform parent, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }
        
        // Overload for the above that takes a prefab
        public static Transform GetDirectChild(Transform parent, GameObject prefab, string name)
        {
            var found = parent.Find(name);

            if (found)
                return found;

            var child = InstantiatePrefab(prefab);
            child.transform.SetParent(parent, false);
            return child.transform;
        }

        public static GameObject InstantiatePrefab(GameObject prefab)
        {
            return PrefabUtility.InstantiatePrefab(prefab) as GameObject
                   ?? throw new Exception("Failed to instantiate prefab");
        }
    }
}