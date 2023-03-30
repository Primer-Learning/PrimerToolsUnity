using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public class GameObjectPool : IPool<Transform>
    {
        private readonly Queue<Transform> pool = new();
        private readonly WeakSet<Transform> inUse = new();

        private readonly string prefabName;
        private Object prefab;

        public void Fill(int amount)
        {
            while (pool.Count < amount)
                pool.Enqueue(Create());
        }

        public Transform Get(Transform parent = null)
        {
            var target = Find(parent);
            var go = target.gameObject;

            if (target.transform.parent != parent)
                target.transform.SetParent(parent, worldPositionStays: true);

            PoolOrchestrator.Reuse(go);

            inUse.Add(target);
            return target;
        }

        private Transform Find(Transform parent)
        {
            Transform result;

            do {
                result = pool.Count != 0
                    ? pool.Dequeue()
                    : PoolOrchestrator.FindPrefab<Transform>(null) ?? Create();
            } while (result == null);

            return result;
        }

        public void Recycle(Transform target)
        {
            if (target == null)
                return;

            PoolOrchestrator.Recycle(target.gameObject);

            inUse.Remove(target);
            pool.Enqueue(target);
        }

        public void RecycleAll()
        {
            foreach (var target in inUse) {
                Recycle(target);
            }
        }

        private static Transform Create()
        {
            var gameObject = new GameObject();
            PoolOrchestrator.OnCreate(gameObject, null);
            return gameObject.transform;
        }
    }
}
