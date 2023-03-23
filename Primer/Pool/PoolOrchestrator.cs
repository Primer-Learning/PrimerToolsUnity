using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    public static class PoolOrchestrator
    {
        private const string POOL_OBJECT_TAG = "PoolObject";
        private static readonly HashSet<GameObject> allPoolObjects = new();
        private static Transform _container;

        private static Transform container {
            get {
                if (_container != null)
                    return _container;

                var go = GameObject.Find("PoolContainer") ?? new GameObject("PoolContainer");
                go.SetActive(false);
                _container = go.transform;
                return _container;
            }
        }

        static PoolOrchestrator()
        {
            UnityTagManager.CreateTag(POOL_OBJECT_TAG);
        }

        public static IEnumerable<T> OrphanObjects<T>(string prefabName)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag(POOL_OBJECT_TAG)) {
                if (allPoolObjects.Contains(go))
                    continue;

                var poolData = go.GetComponent<PoolData>();

                if (poolData is null || poolData.prefabName != prefabName)
                    continue;

                var component = go.GetComponent<T>();

                if (component is null)
                    continue;

                allPoolObjects.Add(go);
                yield return component;
            }
        }

        public static Object GetPrefab(string prefabName)
        {
            var prefab = Resources.Load(prefabName);

            if (prefab is null) {
                throw new Exception($"Cannot find prefab {prefabName}");
            }

            return prefab;
        }

        public static T Create<T>(string prefabName, Object prefab, Transform parent)
        {
            var go = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            var component = go.GetComponent<T>();

            if (component is null) {
                throw new Exception($"Prefab {prefabName} does not have component {typeof(T)}");
            }

            var poolData = go.AddComponent<PoolData>();
            poolData.prefabName = prefabName;

            StageUtility.PlaceGameObjectInCurrentStage(go);
            GameObjectUtility.EnsureUniqueNameForSibling(go);

            allPoolObjects.Add(go);

            return component;
        }

        public static void Reuse(GameObject target)
        {
            GetData(target).state = PoolState.InUse;
        }

        public static void Recycle(GameObject target)
        {
            target.transform.SetParent(container, worldPositionStays: true);
            GetData(target).state = PoolState.Recycled;
        }

        public static T Find<T>(string prefabName) where T : Component
        {
            foreach (var candidate in container.GetChildren()) {


                // if (GetData(orphan.gameObject).state == PoolState.Recycled) {
                //     return orphan;
                // }
            }

            return null;
        }

        private static PoolData GetData(this GameObject go)
        {
            var poolData = go.GetComponent<PoolData>();

            if (poolData is null) {
                throw new Exception($"Object {go.name} does not have PoolObject component");
            }

            return poolData;
        }
    }
}
