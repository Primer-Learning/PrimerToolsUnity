using System;
using System.Collections.Generic;
using System.Linq;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEngine;

namespace Primer
{
    public class Pool : IDisposable
    {
        private readonly GameObject _prefab;
        private static SimpleGnome poolGnome => new ("Pools");
    
        private static readonly Dictionary<CommonPrefabs, Pool> pools = new();
        
        public void Dispose()
        {
            Reset();
            transform.gameObject.SetActive(false);
        }
        
        public void Reset(bool hard = false)
        {
            foreach (var child in transform.GetChildren())
            {
                if (hard)
                    child.gameObject.Dispose();
                else
                    child.gameObject.SetActive(false);
            }
        }

        public Transform transform => poolGnome.Add($"Pool {_prefab.name}");

        #region Add and return

        public Transform GiveToParent(Transform newParent)
        {
            var children = transform.GetChildren();
            GameObject newObject;
            
            // Return unused pooled object if one exists
            if (children.Length > 0)
            { newObject = children.ToArray()[0].gameObject; }
            // Instantiate a prefab if no children exist
            else
            {
                newObject = PrefabUtility.InstantiatePrefab(_prefab) as GameObject;
            }
            if (newObject == null)
                throw new Exception("Failed to instantiate prefab");

            var t = newObject.transform;
            t.SetParent(newParent);
            newObject.gameObject.SetActive(true);
            // These will likely be overridden, but it's a nice default
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            return t;
        }
        
        public void Return(Transform child)
        {
            if (child.name != _prefab.name) Debug.LogWarning($"Returning {child.name} to {transform.name}");
            child.SetParent(transform);
            child.gameObject.SetActive(false);
        }

        #endregion
        
        #region Pool creation
        public static Pool GetPool(CommonPrefabs prefab)
        {
            if (pools.TryGetValue(prefab, out var existingPool))
                return existingPool;
            
            var pool = new Pool(Resources.Load<GameObject>(PrefabFinder.prefabNames[prefab]));
            pools.Add(prefab, pool);
            return pool;
        }
        private Pool(GameObject prefab)
        {
            _prefab = prefab;
            transform.gameObject.SetActive(true);
        }
        #endregion
    }
    public static class PoolExtensions
    {
        public static Transform GetPrefabInstance(this Transform newParent, CommonPrefabs prefab)
        {
            return Pool.GetPool(prefab).GiveToParent(newParent);
        }
    }
}