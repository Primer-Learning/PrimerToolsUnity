using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Primer
{
    public static class PoolOrchestrator
    {
        private const string POOL_OBJECT_TAG = "PoolObject";
        private const string POOL_CONTAINER_NAME = "PoolContainer";
        private static Transform _container;

        private static Transform container {
            get {
                if (_container != null)
                    return _container;

                if (_container is not null)
                    Object.Destroy(_container);

                var go = GameObject.Find(POOL_CONTAINER_NAME) ?? new GameObject(POOL_CONTAINER_NAME);
                go.hideFlags = HideFlags.DontSave;

                // If we disable it GameObject.Find() won't find it
                // go.SetActive(false);

                _container = go.transform;
                return _container;
            }
        }

        static PoolOrchestrator()
        {
            UnityTagManager.CreateTag(POOL_OBJECT_TAG);
        }

        public static T Find<T>(string prefabName) where T : Component
        {
            EnsureOrphansAreCollected();

            return (
                from candidate in container.GetChildren()
                let poolData = candidate.GetComponent<PoolData>()
                where poolData is not null && poolData.prefabName == prefabName
                let component = candidate.GetComponent<T>()
                where component is not null
                select component
            ).FirstOrDefault();
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
            go.tag = POOL_OBJECT_TAG;

            return component;
        }

        public static void Reuse(GameObject target)
        {
            var data = GetData(target);
            data.state = PoolState.InUse;
            target.gameObject.name = data.prefabName;
            GameObjectUtility.EnsureUniqueNameForSibling(target.gameObject);
        }

        public static void Recycle(GameObject target)
        {
            target.transform.SetParent(container, worldPositionStays: true);
            GetData(target).state = PoolState.Recycled;
        }

        private static PoolData GetData(this GameObject go)
        {
            var poolData = go.GetComponent<PoolData>();

            if (poolData is null) {
                throw new Exception($"Object {go.name} does not have PoolObject component");
            }

            return poolData;
        }


        #region Orphans
        private static bool areOrphansCollected;
        private static void EnsureOrphansAreCollected()
        {
            if (areOrphansCollected)
                return;

            areOrphansCollected = true;

            foreach (var go in GameObject.FindGameObjectsWithTag(POOL_OBJECT_TAG)) {
                Recycle(go);
            }
        }
        #endregion
    }
}
